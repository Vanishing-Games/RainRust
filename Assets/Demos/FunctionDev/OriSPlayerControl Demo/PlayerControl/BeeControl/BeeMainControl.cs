using Sirenix.Serialization;
using UnityEngine;

namespace PlayerControlByOris
{
    public enum BeeState
    {
        StaySt,
        FollowSt,
        ThrowedSt,
    }

    public class BeeMainControl : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            player = GameObject.FindWithTag("Player").transform;
            rb = GetComponent<Rigidbody2D>();
            ts = GetComponent<Transform>();
            bc = GetComponent<BoxCollider2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update() { }

        private void FixedUpdate()
        {
            switch (currentState)
            {
                case BeeState.StaySt:
                    StayStUpdate();
                    break;
                case BeeState.ThrowedSt:
                    ThrowedStUpdate();
                    break;
                case BeeState.FollowSt:
                    FollowStUpdate();
                    break;
            }
            currentSpeed = rb.linearVelocity.magnitude;
        }

        public void ChangeState(BeeState toState)
        {
            OnExitState(currentState);

            currentState = toState;

            OnEnterState(currentState);
        }

        void OnExitState(BeeState state)
        {
            switch (currentState)
            {
                case BeeState.StaySt:
                    StayStExit();
                    break;
                case BeeState.ThrowedSt:
                    ThrowedStExit();
                    break;
                case BeeState.FollowSt:
                    FollowStExit();
                    break;
            }
        }

        void OnEnterState(BeeState state)
        {
            switch (currentState)
            {
                case BeeState.StaySt:
                    StayStEnter();
                    break;
                case BeeState.ThrowedSt:
                    ThrowedStEnter();
                    break;
                case BeeState.FollowSt:
                    FollowStEnter();
                    break;
            }
        }

        //投出状态管理
        void ThrowedStEnter() { }

        void ThrowedStUpdate() { }

        void ThrowedStExit() { }

        //跟随状态管理
        void FollowStEnter()
        {
            bc.enabled = false;
        }

        void FollowStUpdate()
        {
            //改变虫子的朝向
            ts.localScale = new Vector3(
                Mathf.Sign(this.transform.position.x - player.position.x),
                1,
                1
            );
            FollowPointSet();

            //过远闪烁
            if (targetDistance() > FlashMoveDistance)
            {
                this.transform.position = FollowPoint;
            }
            //近距离跟随
            else if (targetDistance() <= FlashMoveDistance)
            {
                MoveTowardsTarget(rb, FollowPoint, FollowSpeedMult);
            }
            //围绕近距离跟随时的浮动值移动(先不考虑)
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        void FollowStExit()
        {
            bc.enabled = true;
        }

        //悬挂状态管理
        void StayStEnter()
        {
            //gameObject.layer = LayerMask.GetMask("Hook");
        }

        void StayStUpdate() { }

        void StayStExit()
        {
            //gameObject.layer = LayerMask.GetMask("Bee");
        }

        //外部调用
        public void BeeThrow(Vector2 ThrowVelocity, bool isFaceRight)
        {
            sr.enabled = true;
            ChangeState(BeeState.ThrowedSt);
            rb.linearVelocity = ThrowVelocity;
            FaceDirSet(isFaceRight);
        }

        public void FaceDirSet(bool isFaceRight)
        {
            if (isFaceRight)
                ts.localScale = new Vector3(-1, 1, 1);
            else
                ts.localScale = new Vector3(1, 1, 1);
        }

        public void FlashToPosition(Vector3 positon, bool isHidden)
        {
            //这里在原地留下闪烁特效

            this.transform.position = positon;
            if (isHidden)
                sr.enabled = false;
            else
                sr.enabled = true;
        }

        //其他函数
        void FollowPointSet()
        {
            bool isRight = this.transform.position.x > player.position.x;
            //超出范围时生成一个新点位
            if (Vector3.Distance(FollowPoint, player.position) > FollowPointDistance)
            {
                FollowPoint = GetSidePos(player.position, FollowPointDistance, isRight, 30f);
            }
        }

        void MoveTowardsTarget(Rigidbody2D rb, Vector2 targetPos, float speedMultiplier)
        {
            Vector2 offset = targetPos - (Vector2)transform.position;
            float distance = offset.magnitude;
            rb.linearVelocity = offset.normalized * distance * speedMultiplier;
        }

        //噪声点位生成
        public Vector3 GetNoisePosition(Vector3 basePos, float seed, float freq, float amp)
        {
            // 使用 Time.time 使得噪声随时间平滑推移
            float time = Time.time * freq;

            // 为 X 和 Y 使用不同的采样坐标（seed 确保了个体差异，Offset 确保了维度差异）
            // 我们在 2D 噪声图中，沿着不同的“路径”取值
            float noiseX = Mathf.PerlinNoise(time + seed, 0f);
            float noiseY = Mathf.PerlinNoise(0f, time + seed + 123.45f);

            // 将 0~1 的原始值映射到 -1~1，从而实现以基础点为中心的双向晃动
            float offsetX = (noiseX - 0.5f) * 2f * amp;
            float offsetY = (noiseY - 0.5f) * 2f * amp;

            return basePos + new Vector3(offsetX, offsetY, 0f);
        }

        //初始点位生成
        public Vector3 GetSidePos(
            Vector3 centerPoint,
            float distance,
            bool isRight,
            float angleRange
        )
        {
            // 1. 确定中心角度：右侧为 45度，左侧为 135度
            float centerAngle = isRight ? 15f : 165f;

            // 2. 在范围内随机取一个偏移
            float randomAngle = centerAngle + Random.Range(-angleRange / 2f, angleRange / 2f);

            // 3. 角度转弧度
            float rad = randomAngle * Mathf.Deg2Rad;

            // 4. 计算坐标
            float x = Mathf.Cos(rad) * distance;
            float y = Mathf.Sin(rad) * distance;

            return centerPoint + new Vector3(x, y, 0f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (currentState == BeeState.ThrowedSt && collision.transform.CompareTag("Wall"))
            {
                for (int i = 0, len = collision.contactCount; i < len; i++)
                {
                    Vector2 normal = collision.GetContact(i).normal;
                    if (normal.x <= -0.9f && Mathf.Abs(normal.y) < 0.1f)
                    {
                        ChangeState(BeeState.StaySt);
                    }
                    else if (normal.x >= 0.9f && Mathf.Abs(normal.y) < 0.1f)
                    {
                        ChangeState(BeeState.StaySt);
                    }
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (currentState == BeeState.StaySt && collision.transform.CompareTag("Wall"))
            {
                ChangeState(BeeState.FollowSt);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (currentState == BeeState.StaySt && collision.transform.CompareTag("Wave"))
            {
                ChangeState(BeeState.FollowSt);
            }
        }

        public BeeState currentState;
        private SpriteRenderer sr;
        private Rigidbody2D rb;
        private BoxCollider2D bc;
        public Transform ts;
        private Transform player;
        private Vector2 PlayerPosition;

        public float targetDistance() => Vector3.Distance(this.transform.position, FollowPoint);

        private Vector2 targetDir() => (FollowPoint - this.transform.position).normalized;

        public Vector3 FollowPoint;
        public float FollowPointDistance;
        public float FlashMoveDistance;

        public float FollowSpeedMult;

        public float currentSpeed;
    }
}
