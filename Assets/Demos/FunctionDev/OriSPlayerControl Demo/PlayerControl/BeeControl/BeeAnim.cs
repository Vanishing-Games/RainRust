using UnityEngine;

namespace PlayerControlByOris
{
    public class BeeAnim : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            bmc = GetComponent<BeeMainControl>();
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            anim.SetBool("IsFollow", bmc.currentState == BeeState.FollowSt);
            anim.SetBool("IsThrow", bmc.currentState == BeeState.ThrowedSt);
            anim.SetBool("IsStay", bmc.currentState == BeeState.StaySt);
            anim.SetFloat("Speed", bmc.currentSpeed);
        }

        private Animator anim;
        private BeeMainControl bmc;
    }
}
