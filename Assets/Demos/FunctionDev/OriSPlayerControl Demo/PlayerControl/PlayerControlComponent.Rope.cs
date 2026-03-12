using UnityEngine;
using VanishingGames.ECC.Runtime;

namespace PlayerControlByOris
{
    public partial class PlayerControlComponent : EccComponent
    {
        /// <returns>是否正在丢抓钩</returns>
        public bool IsThrowing() => CurrentState == PlayerStateMachine.ThrowState;

        /// <returns>抓钩的枪口位置</returns>
        public Vector3 GetGrappleTipPosition() => mTranform.position;

        /// <returns>抓钩的终点位置</returns>
        public Vector3 GetGrapplePosition() =>
            ThrownHook ? ThrownHook.transform.position : mTransform.position;
    }
}
