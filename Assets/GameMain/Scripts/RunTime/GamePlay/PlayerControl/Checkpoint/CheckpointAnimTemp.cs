using UnityEngine;

namespace GameMain.RunTime
{
    public class CheckpointAnimTemp : MonoBehaviour
    {
        private Animator anim;
        public bool isBloom;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            anim.SetBool("isBloom", isBloom);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.CompareTag("Player"))
            {
                isBloom = true;
            }
        }
    }
}
