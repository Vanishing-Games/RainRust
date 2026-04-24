using System.Collections.Generic;
using System.Linq;
using Core;
using R3;
using UnityEngine;

namespace GameMain.RunTime
{
    public class SnakeDoor : AutoLdtkEntity
    {
        public override void OnPostImport()
        {
            CheckConditions();
        }

        private void Awake()
        {
            MessageBroker
                .Global.Receive<GamePlaySnakeGameEvents.HoneyCollectedEvent>()
                .Subscribe(_ => CheckConditions())
                .AddTo(ref m_Disposables);

            MessageBroker
                .Global.Receive<GamePlaySnakeGameEvents.HoneyResetEvent>()
                .Subscribe(_ => CheckConditions())
                .AddTo(ref m_Disposables);

            MessageBroker
                .Global.Receive<GamePlaySnakeGameEvents.SnakeDeathEvent>()
                .Subscribe(_ => OnSnakeDeath())
                .AddTo(ref m_Disposables);

            MessageBroker
                .Global.Receive<GamePlaySnakeGameEvents.SnakeSaveEvent>()
                .Subscribe(_ => OnSnakeSave())
                .AddTo(ref m_Disposables);
        }

        private void Start()
        {
            CheckConditions();
        }

        private void OnDestroy()
        {
            m_Disposables.Dispose();
        }

        private void CheckConditions()
        {
            if (m_IsPermanentlyOpen)
                return;

            bool allCollected =
                m_AssociatedHoneys.Count > 0
                && m_AssociatedHoneys.All(h => h.State != HoneyState.Uncollected);
            SetDoorOpen(allCollected);
        }

        private void SetDoorOpen(bool isOpen)
        {
            gameObject.SetActive(!isOpen);
        }

        private void OnSnakeDeath()
        {
            if (!m_IsPermanentlyOpen)
                CheckConditions();
        }

        private void OnSnakeSave()
        {
            if (!gameObject.activeSelf)
                m_IsPermanentlyOpen = true;
        }

        [LDtkField("AssociatedHoneys")]
        [SerializeField]
        private List<SnakeHoney> m_AssociatedHoneys = new();

        private DisposableBag m_Disposables = new();
        private bool m_IsPermanentlyOpen;
    }
}
