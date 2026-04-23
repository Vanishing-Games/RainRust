using UnityEngine;

namespace GameMain.RunTime
{
    public class PlayerSnake : MonoBehaviour
    {
        public void SetUp(SavePoint savePoint)
        {
            gameObject.transform.position = savePoint.transform.position;
        }
    }
}
