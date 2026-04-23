using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using GameMain.RunTime;
using LDtkUnity;
using LDtkUnity.Editor;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    public class LDtkAutoEntityProcessor : LDtkPostprocessor
    {
        public override int GetPostprocessOrder() => 10;

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            if (!root.TryGetComponent<LDtkComponentLevel>(out var level))
            {
                return;
            }

            Transform runtimeContainer = root.transform.Find("RuntimeEntities");
            if (runtimeContainer == null)
            {
                runtimeContainer = new GameObject("RuntimeEntities").transform;
                runtimeContainer.SetParent(root.transform);
                runtimeContainer.localPosition = Vector3.zero;
            }
            else
            {
                for (int i = runtimeContainer.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(runtimeContainer.GetChild(i).gameObject);
            }

            LDtkComponentEntity[] entities = root.GetComponentsInChildren<LDtkComponentEntity>();
            foreach (var ldtkEntity in entities)
            {
                if (!ldtkEntity.TryGetComponent<LDtkFields>(out var fields))
                {
                    continue;
                }

                if (!fields.TryGetBool("AutoUnityEntity", out bool autoProcess) || !autoProcess)
                {
                    continue;
                }

                if (
                    !fields.TryGetString("EntityPrefab", out string prefabRelPath)
                    || string.IsNullOrEmpty(prefabRelPath)
                )
                {
                    CLogger.LogWarn(
                        $"[LDtkAutoEntity] Entity {ldtkEntity.Identifier} marked for auto-process but 'EntityPrefab' is missing or empty.",
                        LogTag.LDtkAutoEntityProcessor
                    );
                    continue;
                }

                string fullPrefabPath = Path.Combine("Assets/Prefabs", prefabRelPath + ".prefab")
                    .Replace("\\", "/");
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fullPrefabPath);

                if (prefabAsset == null)
                {
                    CLogger.LogError(
                        $"[LDtkAutoEntity] Cannot find prefab at path: {fullPrefabPath}",
                        LogTag.LDtkAutoEntityProcessor
                    );
                    continue;
                }

                GameObject newEntity = (GameObject)
                    PrefabUtility.InstantiatePrefab(prefabAsset, runtimeContainer);
                newEntity.transform.SetPositionAndRotation(
                    ldtkEntity.transform.position,
                    ldtkEntity.transform.rotation
                );
                newEntity.transform.localScale = ldtkEntity.transform.localScale;
                newEntity.name = $"{prefabAsset.name}_{ldtkEntity.Iid.Iid}";

                if (newEntity.TryGetComponent<LDtkEntityDataHandler>(out var handler))
                {
                    ProcessFields(fields, handler);
                }

                if (fields.TryGetBool("EntityResizable", out bool resizable) && resizable)
                {
                    var resizableEntity = newEntity.GetComponentInChildren<ILdtkResizableEntity>();
                    if (resizableEntity != null)
                    {
                        Vector2 size = ldtkEntity.Size;
                        resizableEntity.ReSize(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
                    }
                    else
                    {
                        CLogger.LogError(
                            $"[LDtkAutoEntity] Entity {ldtkEntity.Identifier} has EntityResizable=true but no ILdtkResizableEntity found on prefab '{prefabAsset.name}'.",
                            LogTag.LDtkAutoEntityProcessor
                        );
                    }
                }
                else
                {
                    // 由于 LDtk 的实体默认锚点在左上角，而 Unity 的默认锚点在中心，如果不调整位置，可能会导致实体位置偏移。
                    // 按照实体的大小调整位置，使其锚点与 LDtk 一致。
                    Vector2 size = ldtkEntity.Size;
                    newEntity.transform.position += new Vector3(size.x * 0.5f, -size.y * 0.5f, 0);
                }

                ldtkEntity.gameObject.SetActive(false);
            }
        }

        private void ProcessFields(LDtkFields fields, LDtkEntityDataHandler handler)
        {
            handler.ClearRecords();

            foreach (var field in fields.Fields)
            {
                if (!field.Identifier.StartsWith("_"))
                {
                    continue;
                }

                var record = new LDtkEntityDataHandler.DataRecord
                {
                    Identifier = field.Identifier,
                    Type = field.Type,
                    IsArray = field.IsArray,
                };

                if (!field.IsArray)
                {
                    ExtractSingleFieldValue(fields, field, record);
                }
                else
                {
                    ExtractArrayFieldValue(fields, field, record);
                }

                handler.AddRecord(record);
            }

            EditorUtility.SetDirty(handler);
        }

        private void ExtractSingleFieldValue(
            LDtkFields fields,
            LDtkField field,
            LDtkEntityDataHandler.DataRecord record
        )
        {
            string id = field.Identifier;
            switch (field.Type)
            {
                case LDtkFieldType.Int:
                    record.IntValue = fields.GetInt(id);
                    break;
                case LDtkFieldType.Float:
                    record.FloatValue = fields.GetFloat(id);
                    break;
                case LDtkFieldType.Bool:
                    record.BoolValue = fields.GetBool(id);
                    break;
                case LDtkFieldType.String:
                case LDtkFieldType.Multiline:
                case LDtkFieldType.FilePath:
                    record.StringValue = fields.GetString(id);
                    break;
                case LDtkFieldType.Color:
                    record.ColorValue = fields.GetColor(id);
                    break;
                case LDtkFieldType.Enum:
                    record.StringValue = fields.GetValueAsString(id);
                    break;
                case LDtkFieldType.Point:
                    record.VectorValue = fields.GetPoint(id);
                    break;
                case LDtkFieldType.EntityRef:
                    record.EntityRef = fields.GetEntityReference(id);
                    break;
            }
        }

        private void ExtractArrayFieldValue(
            LDtkFields fields,
            LDtkField field,
            LDtkEntityDataHandler.DataRecord record
        )
        {
            string id = field.Identifier;
            switch (field.Type)
            {
                case LDtkFieldType.Int:
                    record.IntArray = fields.GetIntArray(id).ToList();
                    break;
                case LDtkFieldType.Float:
                    record.FloatArray = fields.GetFloatArray(id).ToList();
                    break;
                case LDtkFieldType.Bool:
                    record.BoolArray = fields.GetBoolArray(id).ToList();
                    break;
                case LDtkFieldType.String:
                case LDtkFieldType.Multiline:
                case LDtkFieldType.FilePath:
                    record.StringArray = fields.GetStringArray(id).ToList();
                    break;
                case LDtkFieldType.Color:
                    record.ColorArray = fields.GetColorArray(id).ToList();
                    break;
                case LDtkFieldType.Enum:
                    record.StringArray = fields.GetValuesAsStrings(id).ToList();
                    break;
                case LDtkFieldType.Point:
                    record.VectorArray = fields.GetPointArray(id).ToList();
                    break;
                case LDtkFieldType.EntityRef:
                    record.EntityRefArray = fields.GetEntityReferenceArray(id).ToList();
                    break;
            }
        }
    }
}
