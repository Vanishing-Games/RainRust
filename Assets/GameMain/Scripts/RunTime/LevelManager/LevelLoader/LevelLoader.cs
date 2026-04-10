using Core;
using Cysharp.Threading.Tasks;

namespace GameMain.RunTime
{
    public class LevelLoader : LoaderBase<LevelLoadInfo>
    {
        private LevelLoadInfo m_LevelLoadInfo;

        public override LoaderType GetLoaderType()
        {
            return LoaderType.LevelLoader;
        }

        public override void InitLoader(LevelLoadInfo loadInfo)
        {
            m_LevelLoadInfo = loadInfo;
        }

        public override async UniTask LoadResource()
        {
            LevelManager.Instance.StartLevel(
                m_LevelLoadInfo.ChapterId,
                m_LevelLoadInfo.LevelId,
                m_LevelLoadInfo.LevelSpawnPointIndex
            );
            await UniTask.Yield();
        }
    }
}
