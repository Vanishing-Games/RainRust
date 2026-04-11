using System;
using Cysharp.Threading.Tasks;

namespace Core
{
    public interface IGameCoreHookRegistry
    {

        void OnBootStart(Func<UniTask> handler, int order = 0);

        void OnLoadStart(Func<LoadContext, UniTask> handler);

        void OnLoadScene(Func<LoadContext, UniTask> handler);

        void OnLoadComplete(Func<LoadContext, UniTask> handler);

        void OnMainMenuEnter(Func<UniTask> handler);

        void OnMainMenuExit(Func<UniTask> handler);

        void OnInLevelEnter(Func<LoadContext, UniTask> handler);

        void OnInLevelExit(Func<UniTask> handler);

        void OnUpdate(Action handler);

        void OnGameQuit(Func<UniTask> handler);
    }
}
