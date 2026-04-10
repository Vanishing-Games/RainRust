using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanishingGames.ECC.Runtime;

namespace GameMain.RunTime
{
    public class PlayerStMachineWhistle : PlayerControlCapabilityBase
    {
        protected override void SetUpTickSettings()
        {
            base.SetUpTickSettings();
            TickOrderInGroup = (uint)PlayerControlTickOrder.WhistleControl;
        }

        protected override bool OnShouldActivate()
        {
            return CanWhistleCheck();
        }

        protected override void OnActivate()
        {
            SetStateMachine(PlayerStateMachine.WhistleState, EccTag.WhistleState);
            mPCComponent.WhistleBeforeTimer = mPCComponent.WhistleBeforeTime;
            mPCComponent.WhistleStayTimer = mPCComponent.WhistleStayTime;
            mPCComponent.WhistleAfterTimer = mPCComponent.WhistleAfterTime;
            mPCComponent.CtrlVelocity = Vector2.zero;
        }

        protected override bool OnShouldDeactivate()
        {
            return mPCComponent.WhistleAfterTimer == 0 || !mPCComponent.IsOnGround;
        }

        protected override void OnDeactivate()
        {
            SetStateMachine(PlayerStateMachine.NormalState, EccTag.NormalState);
            mPCComponent.WhistleBeforeTimer = 0;
            mPCComponent.WhistleStayTimer = 0;
            mPCComponent.WhistleAfterTimer = 0;
        }

        protected override void OnTick(float deltaTime)
        {
            if (mPCComponent.WhistleBeforeTimer > 0)
                mPCComponent.WhistleBeforeTimer--;

            if (mPCComponent.WhistleBeforeTimer == 0 && mPCComponent.WhistleStayTimer > 0)
                mPCComponent.WhistleStayTimer--;

            //条件待修改
            if (!mPCComponent.InputAct2 && mPCComponent.WhistleStayTimer == 0)
            {
                //只在第一帧生成一次
                if (mPCComponent.WhistleAfterTimer == mPCComponent.WhistleAfterTime)
                    CreateWave();
                mPCComponent.WhistleAfterTimer--;
            }
        }

        void CreateWave()
        {
            Object.Instantiate(
                mPCComponent.PreWave,
                mPCComponent.mTranform.position,
                Quaternion.identity
            );
        }

        private bool CanWhistleCheck() =>
            IsOnGround
            && mPCComponent.CurrentState == PlayerStateMachine.NormalState
            && mPCComponent.PreWhistleInputTimer > 0
            && mPCComponent.PreWhistleInputTimer < mPCComponent.PreWhistleInputTime;
    }
}
