using Core;
using UnityEngine;

namespace GameMain.RunTime
{
    public abstract class ScalableEntity : MonoBehaviour, ILdtkResizableEntity
    {
        public virtual void ReSize(int x, int y)
        {
            transform.localScale = new Vector3(x, y, 1);
            transform.MoveInWorldCoordDiscardHierachyTransformatino(
                new Vector3(x * 0.5f, -y * 0.5f, 0)
            );
        }
    }

    public interface ILdtkResizableEntity
    {
        void ReSize(int x, int y);
    }
}
