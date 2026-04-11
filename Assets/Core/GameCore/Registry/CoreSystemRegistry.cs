using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Core
{
    public class CoreSystemRegistry : IGameCoreHookRegistry
    {
        public void Register(ICoreModuleSystem system)
        {
            m_Systems[system.GetType()] = system;
            m_CurrentRegisteringSystemType = system.GetType();
            system.RegisterHooks(this);
            m_CurrentRegisteringSystemType = null;
        }

        public async UniTask InitializeAllAsync()
        {
            foreach (var system in GetSortedSystems())
            {
                CLogger.LogInfo(
                    $"[CoreSystemRegistry] Initializing: {system.SystemName}",
                    LogTag.GameCoreStart
                );
                if (m_InitHandlers.TryGetValue(system.GetType(), out var handlers))
                {
                    foreach (var handler in handlers)
                        await handler();
                }
            }
        }

        public async UniTask FireOnBootStart()
        {
            var sortedBootHandlers = m_BootHandlers.OrderBy(x => x.Order).Select(x => x.Handler);
            foreach (var handler in sortedBootHandlers)
                await handler();
        }

        public async UniTask FireOnLoadStart(LoadContext ctx) =>
            await FireAll(m_LoadStartHandlers, ctx);

        public async UniTask FireOnLoadScene(LoadContext ctx) =>
            await FireAll(m_LoadSceneHandlers, ctx);

        public async UniTask FireOnLoadComplete(LoadContext ctx) =>
            await FireAll(m_LoadCompleteHandlers, ctx);

        public async UniTask FireOnMainMenuEnter() => await FireAll(m_MainMenuEnterHandlers);

        public async UniTask FireOnMainMenuExit() => await FireAll(m_MainMenuExitHandlers);

        public async UniTask FireOnInLevelEnter(LoadContext ctx) =>
            await FireAll(m_InLevelEnterHandlers, ctx);

        public async UniTask FireOnInLevelExit() => await FireAll(m_InLevelExitHandlers);

        public async UniTask FireOnGameQuit() => await FireAll(m_GameQuitHandlers);

        public void FireOnUpdate()
        {
            foreach (var system in GetSortedSystems())
            {
                if (m_UpdateHandlers.TryGetValue(system.GetType(), out var handlers))
                {
                    foreach (var handler in handlers)
                        handler();
                }
            }
        }

        private async UniTask FireAll(Dictionary<Type, List<Func<UniTask>>> registry)
        {
            foreach (var system in GetSortedSystems())
            {
                if (registry.TryGetValue(system.GetType(), out var handlers))
                {
                    foreach (var handler in handlers)
                        await handler();
                }
            }
        }

        private async UniTask FireAll(
            Dictionary<Type, List<Func<LoadContext, UniTask>>> registry,
            LoadContext ctx
        )
        {
            foreach (var system in GetSortedSystems())
            {
                if (registry.TryGetValue(system.GetType(), out var handlers))
                {
                    foreach (var handler in handlers)
                        await handler(ctx);
                }
            }
        }

        public void OnBootStart(Func<UniTask> handler, int order = 0) =>
            m_BootHandlers.Add((order, handler));

        public void OnLoadStart(Func<LoadContext, UniTask> handler) =>
            GetOrCreate(m_LoadStartHandlers).Add(handler);

        public void OnLoadScene(Func<LoadContext, UniTask> handler) =>
            GetOrCreate(m_LoadSceneHandlers).Add(handler);

        public void OnLoadComplete(Func<LoadContext, UniTask> handler) =>
            GetOrCreate(m_LoadCompleteHandlers).Add(handler);

        public void OnMainMenuEnter(Func<UniTask> handler) =>
            GetOrCreate(m_MainMenuEnterHandlers).Add(handler);

        public void OnMainMenuExit(Func<UniTask> handler) =>
            GetOrCreate(m_MainMenuExitHandlers).Add(handler);

        public void OnInLevelEnter(Func<LoadContext, UniTask> handler) =>
            GetOrCreate(m_InLevelEnterHandlers).Add(handler);

        public void OnInLevelExit(Func<UniTask> handler) =>
            GetOrCreate(m_InLevelExitHandlers).Add(handler);

        public void OnUpdate(Action handler) => GetOrCreate(m_UpdateHandlers).Add(handler);

        public void OnGameQuit(Func<UniTask> handler) =>
            GetOrCreate(m_GameQuitHandlers).Add(handler);

        private List<H> GetOrCreate<H>(Dictionary<Type, List<H>> registry)
        {
            if (m_CurrentRegisteringSystemType == null)
                throw new InvalidOperationException(
                    "Hook registration must happen during Register()"
                );

            if (!registry.TryGetValue(m_CurrentRegisteringSystemType, out var list))
            {
                list = new List<H>();
                registry[m_CurrentRegisteringSystemType] = list;
            }
            return list;
        }

        private List<ICoreModuleSystem> GetSortedSystems()
        {
            var inDegree = m_Systems.Keys.ToDictionary(t => t, _ => 0);
            var dependents = new Dictionary<Type, List<Type>>();

            foreach (var system in m_Systems.Values)
            {
                foreach (var dep in system.Dependencies)
                {
                    if (!m_Systems.ContainsKey(dep))
                        continue;
                    if (!dependents.ContainsKey(dep))
                        dependents[dep] = new List<Type>();
                    dependents[dep].Add(system.GetType());
                    inDegree[system.GetType()]++;
                }
            }

            var queue = new Queue<Type>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var result = new List<ICoreModuleSystem>();

            while (queue.Count > 0)
            {
                var type = queue.Dequeue();
                result.Add(m_Systems[type]);
                if (dependents.TryGetValue(type, out var deps))
                {
                    foreach (var dep in deps)
                    {
                        if (--inDegree[dep] == 0)
                            queue.Enqueue(dep);
                    }
                }
            }

            if (result.Count != m_Systems.Count)
                throw new InvalidOperationException(
                    "[CoreSystemRegistry] Circular dependency detected among systems"
                );

            return result;
        }

        private readonly Dictionary<Type, ICoreModuleSystem> m_Systems = new();
        private Type m_CurrentRegisteringSystemType;

        private readonly Dictionary<Type, List<Func<UniTask>>> m_InitHandlers = new();
        private readonly List<(int Order, Func<UniTask> Handler)> m_BootHandlers = new();
        private readonly Dictionary<Type, List<Func<LoadContext, UniTask>>> m_LoadStartHandlers =
            new();
        private readonly Dictionary<Type, List<Func<LoadContext, UniTask>>> m_LoadSceneHandlers =
            new();
        private readonly Dictionary<Type, List<Func<LoadContext, UniTask>>> m_LoadCompleteHandlers =
            new();
        private readonly Dictionary<Type, List<Func<UniTask>>> m_MainMenuEnterHandlers = new();
        private readonly Dictionary<Type, List<Func<UniTask>>> m_MainMenuExitHandlers = new();
        private readonly Dictionary<Type, List<Func<LoadContext, UniTask>>> m_InLevelEnterHandlers =
            new();
        private readonly Dictionary<Type, List<Func<UniTask>>> m_InLevelExitHandlers = new();
        private readonly Dictionary<Type, List<Action>> m_UpdateHandlers = new();
        private readonly Dictionary<Type, List<Func<UniTask>>> m_GameQuitHandlers = new();
    }
}
