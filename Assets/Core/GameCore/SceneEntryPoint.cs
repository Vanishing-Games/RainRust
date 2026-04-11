using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class SceneEntryPoint : MonoBehaviour
    {
        private void Start()
        {
            GameCore.Instance.OnSceneEntryPointReady(this).Forget();
        }

        public LoadContext GetStandaloneContext() =>
            LoadContext.ForStandalone(
                m_StandaloneChapterId,
                m_StandaloneLevelId,
                m_StandaloneSpawnIndex
            );

        public GameFlowState TargetState => m_TargetState;

        [SerializeField]
        private GameFlowState m_TargetState = GameFlowState.Booting;

        [SerializeField]
        private string m_StandaloneChapterId;

        [SerializeField]
        private string m_StandaloneLevelId;

        [SerializeField]
        private int m_StandaloneSpawnIndex;
    }
}
