using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameMain.RunTime
{
    /// <summary>
    /// Handles LDtk entity data for a MovePlatformer.
    /// Expects LDtk fields: _MoveSpeed (Float), _WaitTime (Float), _LoopType (Enum), _AutoStart (Bool), _PathPoints (PointRefArray).
    /// </summary>
    [RequireComponent(typeof(MovePlatformer))]
    public class MovePlatformerLdtkLoadHandler : LDtkEntityDataHandler
    {
        private MovePlatformer m_Platformer;

        private void Awake()
        {
            m_Platformer = GetComponent<MovePlatformer>();
        }

        protected override void OnSetFloat(string key, float value)
        {
            if (m_Platformer == null)
                return;

            switch (key)
            {
                case "MoveSpeed":
                    m_Platformer.SetMoveSpeed(value);
                    break;
                case "WaitTime":
                    m_Platformer.SetWaitTime(value);
                    break;
            }
        }

        protected override void OnSetBool(string key, bool value)
        {
            if (m_Platformer == null)
                return;

            if (key == "AutoStart")
            {
                m_Platformer.SetAutoStart(value);
            }
        }

        protected override void OnSetEnum(string key, string enumValue)
        {
            if (m_Platformer == null)
                return;

            if (key == "LoopType")
            {
                if (Enum.TryParse<MovePlatformer.LoopType>(enumValue, true, out var loopType))
                {
                    m_Platformer.SetLoopType(loopType);
                }
            }
        }

        protected override void OnSetEntityArray(string key, GameObject[] entities)
        {
            if (m_Platformer == null)
                return;

            if (key == "PathPoints")
            {
                // Convert Vector2 points to Vector3 for the platformer.
                // Note: LDtkUnity usually provides these in world space or level-local space
                // depending on settings, but LDtkAutoEntityProcessor passes them through as Vector2.
                List<Vector3> points = entities.Select(e => e.transform.position).ToList();
                m_Platformer.SetPathPoints(points);
            }
        }

        // override OnSetEntityArray
    }
}
