using Core;

namespace GameMain.RunTime
{
    public class LevelLoadInfo : ILoadInfo
    {
        public string ChapterId { get; private set; }
        public string LevelId { get; private set; }
        public int LevelSpawnPointIndex { get; private set; }

        public LevelLoadInfo(string chapterId, string levelId, int levelSpawnPointIndex)
        {
            ChapterId = chapterId;
            LevelId = levelId;
            LevelSpawnPointIndex = levelSpawnPointIndex;
        }

        public LoaderType GetNeededLoaderType()
        {
            return LoaderType.LevelLoader;
        }
    }
}
