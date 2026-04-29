using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameMain.Editor
{
    public class LDtkAutoEntityProcessor : LDtkPostprocessor
    {
        public override int GetPostprocessOrder() => 100;

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            if (!root.TryGetComponent<LDtkComponentLevel>(out var level))
                return;

            Transform runtimeContainer = GetOrCreateRuntimeContainer(root);
            LDtkComponentEntity[] ldtkEntities =
                root.GetComponentsInChildren<LDtkComponentEntity>();

            var pending =
                new List<(
                    RunTime.AutoLdtkEntity entity,
                    LDtkFields fields,
                    LDtkComponentEntity ldtkEntity
                )>();

            var pivotByIdentifier = projectJson.Defs.Entities.ToDictionary(
                e => e.Identifier,
                e => e.UnityPivot
            );

            // Pass 1: instantiate all prefabs and register IIDs so entity refs can resolve in pass 2
            foreach (var ldtkEntity in ldtkEntities)
            {
                if (!ldtkEntity.gameObject.activeSelf)
                    continue;

                ldtkEntity.gameObject.SetActive(false);

                ldtkEntity.TryGetComponent<LDtkFields>(out var fields);

                string identifier = ldtkEntity.Identifier;
                GameObject prefabAsset = FindEntityPrefab(identifier, fields);
                if (prefabAsset == null)
                {
                    CLogger.LogError(
                        $"[LDtkAutoEntity] Prefab '{identifier}' not found in 'Assets/Prefabs/RunTime'.",
                        LogTag.LDtkAutoEntityProcessor
                    );
                    continue;
                }

                GameObject instance = (GameObject)
                    PrefabUtility.InstantiatePrefab(prefabAsset, runtimeContainer);

                SetInstanceTransform(instance, ldtkEntity);

                instance.name = $"{identifier}_{ldtkEntity.Iid.Iid}";

                RuntimeEntityRegistry.Register(ldtkEntity.Iid.Iid, instance);

                if (!instance.TryGetComponent<AutoLdtkEntity>(out var entityComp))
                {
                    CLogger.LogError(
                        $"[LDtkAutoEntity] Prefab '{identifier}' has no LDtkEntity component.",
                        LogTag.LDtkAutoEntityProcessor
                    );
                    Object.DestroyImmediate(instance);
                    continue;
                }

                entityComp.LdtkIid = ldtkEntity.Iid.Iid;
                entityComp.Level = ldtkEntity.GetComponentInParent<LDtkComponentLevel>();
                entityComp.World = ldtkEntity.GetComponentInParent<LDtkComponentWorld>();

                pending.Add((entityComp, fields, ldtkEntity));
            }

            // Pass 2: inject fields (entity refs now resolvable) then sync and post-import
            foreach (var (entityComp, fields, ldtkEntity) in pending)
            {
                if (fields != null)
                {
                    InjectLDtkFields(entityComp, fields);
                }

                if (entityComp.OnSyncFromLdtk(ldtkEntity))
                {
                    entityComp.OnPostImport();
                    EditorUtility.SetDirty(entityComp);
                }
            }
        }

        private void SetInstanceTransform(GameObject instance, LDtkComponentEntity entity)
        {
            var autoEntity = instance.GetComponent<AutoLdtkEntity>();
            if (autoEntity == null || autoEntity.CanResize)
                return;

            var upLeftWorldPos = entity.transform.position;
            var entitySize = entity.Size;
            var finalPos = upLeftWorldPos + new Vector3(entitySize.x * 0.5f, entitySize.y * -0.5f);

            instance.transform.position = finalPos;
        }

        private Transform GetOrCreateRuntimeContainer(GameObject root)
        {
            Transform container = root.transform.Find("RuntimeEntities");
            if (container == null)
            {
                container = new GameObject("RuntimeEntities").transform;
                container.SetParent(root.transform);
                container.localPosition = Vector3.zero;
            }
            else
            {
                for (int i = container.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(container.GetChild(i).gameObject);
            }
            return container;
        }

        private GameObject FindEntityPrefab(string identifier, LDtkFields fields)
        {
            if (
                fields != null
                && fields.TryGetString("EntityPrefab", out string manualPath)
                && !string.IsNullOrEmpty(manualPath)
            )
            {
                string fullPath = Path.Combine("Assets/Prefabs", manualPath + ".prefab")
                    .Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                if (asset != null)
                    return asset;
                CLogger.LogError(
                    $"[LDtkAutoEntity] Manual Prefab path not found: {fullPath}",
                    LogTag.LDtkAutoEntityProcessor
                );
            }

            string[] guids = AssetDatabase.FindAssets(
                $"{identifier} t:Prefab",
                new[] { "Assets/Prefabs/RunTime" }
            );
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (
                    Path.GetFileNameWithoutExtension(path)
                        .Equals(identifier, StringComparison.OrdinalIgnoreCase)
                )
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            return null;
        }

        private void InjectLDtkFields(RunTime.AutoLdtkEntity target, LDtkFields ldtkFields)
        {
            var type = target.GetType();
            var fields = type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<RunTime.LDtkFieldAttribute>();
                if (attr == null)
                    continue;

                string ldtkKey = attr.CustomIdentifier ?? field.Name;
                if (!ldtkFields.ContainsField(ldtkKey))
                {
                    if (ldtkFields.ContainsField("_" + ldtkKey))
                        ldtkKey = "_" + ldtkKey;
                    else
                    {
                        CLogger.LogWarn(
                            $"[LDtkAutoEntity] Field '{ldtkKey}' not found in LDtk entity {target.name}",
                            LogTag.LDtkAutoEntityProcessor
                        );
                        continue;
                    }
                }

                try
                {
                    object value = GetValueFromLDtk(ldtkFields, ldtkKey, field.FieldType);
                    if (value != null)
                        field.SetValue(target, value);
                }
                catch (Exception e)
                {
                    CLogger.LogError(
                        $"[LDtkAutoEntity] Failed to inject field '{ldtkKey}' into {target.name}: {e.Message}",
                        LogTag.LDtkAutoEntityProcessor
                    );
                }
            }
        }

        private object GetValueFromLDtk(LDtkFields fields, string key, Type targetType)
        {
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = targetType.GetGenericArguments()[0];
                return GetArrayValue(fields, key, elementType);
            }
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType();
                var list = GetArrayValue(fields, key, elementType);
                if (list == null)
                    return null;
                MethodInfo toArrayMethod = typeof(Enumerable)
                    .GetMethod("ToArray")
                    .MakeGenericMethod(elementType);
                return toArrayMethod.Invoke(null, new[] { list });
            }

            if (targetType == typeof(int))
                return fields.GetInt(key);
            if (targetType == typeof(float))
                return fields.GetFloat(key);
            if (targetType == typeof(bool))
                return fields.GetBool(key);
            if (targetType == typeof(string))
                return fields.GetString(key);
            if (targetType == typeof(Color))
                return fields.GetColor(key);
            if (targetType == typeof(Vector2))
                return fields.GetPoint(key);
            if (targetType.IsEnum)
            {
                string enumStr = fields.GetValueAsString(key);
                try
                {
                    return Enum.Parse(targetType, enumStr, true);
                }
                catch
                {
                    return null;
                }
            }
            if (targetType == typeof(GameObject))
            {
                var @ref = fields.GetEntityReference(key);
                return @ref != null
                    ? RunTime.RuntimeEntityRegistry.GetEntity(@ref.EntityIid)
                    : null;
            }
            if (typeof(Component).IsAssignableFrom(targetType))
            {
                var @ref = fields.GetEntityReference(key);
                if (@ref == null)
                    return null;
                var go = RunTime.RuntimeEntityRegistry.GetEntity(@ref.EntityIid);
                return go?.GetComponent(targetType);
            }

            return null;
        }

        private object GetArrayValue(LDtkFields fields, string key, Type elementType)
        {
            if (elementType == typeof(int))
                return fields.GetIntArray(key).ToList();
            if (elementType == typeof(float))
                return fields.GetFloatArray(key).ToList();
            if (elementType == typeof(bool))
                return fields.GetBoolArray(key).ToList();
            if (elementType == typeof(string))
                return fields.GetStringArray(key).ToList();
            if (elementType == typeof(Color))
                return fields.GetColorArray(key).ToList();
            if (elementType == typeof(Vector2))
                return fields.GetPointArray(key).ToList();
            if (
                elementType == typeof(GameObject)
                || typeof(Component).IsAssignableFrom(elementType)
            )
            {
                var refs = fields.GetEntityReferenceArray(key);
                if (refs == null)
                    return null;
                var list = (IList)
                    Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                foreach (var r in refs)
                {
                    var go = RunTime.RuntimeEntityRegistry.GetEntity(r.EntityIid);
                    if (go == null)
                        continue;
                    object item =
                        elementType == typeof(GameObject)
                            ? (object)go
                            : go.GetComponent(elementType);
                    if (item != null)
                        list.Add(item);
                }
                return list;
            }

            return null;
        }
    }
}
