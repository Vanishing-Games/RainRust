#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;

namespace Core
{
    public partial class GameCore : MonoSingletonPersistent<GameCore>
    {
        public enum GameCoreMode
        {
            RunTime,
            EditorFast,
        }

        private bool GameRunInEditorCheck()
        {
            var allSubclasses = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types.Where(t => t != null);
                    }
                })
                .Where(t =>
                    t != null
                    && t.BaseType?.IsGenericType == true
                    && t.BaseType.GetGenericTypeDefinition() == typeof(CoreModuleManagerBase<>)
                )
                .ToList();

            bool hasOffending = false;
            foreach (var t in allSubclasses)
            {
                var methods = t.GetMethods(
                        BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.DeclaredOnly
                    )
                    .Select(m => m.Name)
                    .ToList();

                var offending = methods.Where(m => UnityLifecycleMethods.Contains(m)).ToList();

                if (offending.Count > 0)
                {
                    CLogger.LogError(
                        $"Game Running Condition failed: Class '{t.FullName}' declares Unity lifecycle methods: {string.Join(", ", offending)}. All declared methods: {string.Join(", ", methods)}",
                        LogTag.GameRunCheck
                    );
                    hasOffending = true;
                }
            }

            return !hasOffending;
        }

        private static readonly string[] UnityLifecycleMethods =
        {
            "Awake",
            "Start",
            "Update",
            "FixedUpdate",
            "LateUpdate",
            "OnEnable",
            "OnDisable",
            "OnDestroy",
        };

        public GameCoreMode m_GameCoreMode = GameCoreMode.RunTime;
    }
}

#endif
