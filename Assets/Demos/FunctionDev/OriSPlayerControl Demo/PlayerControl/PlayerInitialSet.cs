using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanishingGames.ECC.Runtime;

namespace PlayerControlByOris
{
    public class PlayerInitialSet : PlayerControlCapabilityBase
    {
        protected override void SetUpTickSettings()
        {
            base.SetUpTickSettings();
            TickOrderInGroup = (uint)PlayerControlTickOrder.InitialSet;
        }

        protected override void OnSetup()
        {
            base.OnSetup();
            mPCComponent.FacingDir = 1;
            StateTag = this;
            SetStateMachine(PlayerStateMachine.NormalState, EccTag.NormalState);
            if (mPCComponent.AllBees[0] == null || mPCComponent.AllBees.Count == 0)
            {
                var tempbee = GameObject.Find("Bee");
                if (mPCComponent.AllBees.Count == 0)
                    mPCComponent.AllBees.Add(tempbee);
                else
                    mPCComponent.AllBees[0] = tempbee;
            }
        }

        protected override bool OnShouldActivate()
        {
            return true;
        }

        protected override bool OnShouldDeactivate()
        {
            return false;
        }

        protected override void OnTick(float deltaTime) { }
    }
}
