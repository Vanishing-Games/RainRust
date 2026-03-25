using UnityEngine;
using VanishingGames.ECC.Runtime;

namespace PlayerControlByOris
{
    public partial class PlayerControlComponent : EccComponent
    {
        /// <returns>是否正在丢抓钩</returns>
        public bool IsThrowing() =>
            (
                CurrentState == PlayerStateMachine.ThrowState
                && ThrowStartTimer == 0
                && ThrowMoveTimer >= 0
            )
            || CurrentState == PlayerStateMachine.DashState;

        /// <returns>抓钩的枪口位置</returns>
        public Vector3 GetGrappleTipPosition() => mTranform.position - ThrowStartOffset;

        /// <returns>抓钩的终点位置</returns>
        public Vector3 GetGrapplePosition() =>
            BeeToThrow ? BeeToThrow.ts.position : mTransform.position;
    }
}
