#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;

namespace Core
{
    public partial class GameCore : MonoSingletonPersistent<GameCore>
    {
        private bool GameRunInEditorCheck()
        {
            bool hasUnityLifecycleMethods = AppDomain
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
                .Any(t =>
                    t != null
                    && t.BaseType?.IsGenericType == true
                    && t.BaseType.GetGenericTypeDefinition() == typeof(CoreModuleManagerBase<,,>)
                    && t.GetMethods(
                            BindingFlags.Instance
                                | BindingFlags.Public
                                | BindingFlags.NonPublic
                                | BindingFlags.DeclaredOnly
                        )
                        .Any(m => UnityLifecycleMethods.Contains(m.Name))
                );

            if (hasUnityLifecycleMethods)
            {
                CLogger.LogError(
                    $"Game Running Condition failed, beacause unity lifecycle methods are found in class inherit from CoreModuleManagerBase",
                    LogTag.GameRunCheck
                );
            }

            return !hasUnityLifecycleMethods;
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
    }
}


#endif
