using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameMain.RunTime
{
    public class EntityChain : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private GameObject m_JointPrefab;

        [SerializeField]
        private int m_ChainLength = 5;

        [Tooltip("Distance between each joint center (match joint prefab height)")]
        [SerializeField]
        private float m_JointSpacing = 1f;

        [Header("Runtime State")]
        [SerializeField]
        private List<GameObject> m_Joints = new();

        public IReadOnlyList<GameObject> Joints => m_Joints;

        private GameObject m_Anchor;

        private void Start()
        {
            if (m_Joints.Count != m_ChainLength)
                Setup();
        }

        public void Setup()
        {
            Clear();

            if (m_JointPrefab == null)
            {
                Debug.LogError($"[EntityChain] JointPrefab is not set on {gameObject.name}.", this);
                return;
            }

            float halfSpacing = m_JointSpacing * 0.5f;

            // Create anchor: no SpriteRenderer, same position as joint_0
            m_Anchor = new GameObject("ChainAnchor");
            m_Anchor.transform.SetParent(transform);
            m_Anchor.transform.localPosition = Vector3.zero;

            var anchorRb = m_Anchor.AddComponent<Rigidbody2D>();
            anchorRb.bodyType = RigidbodyType2D.Dynamic;

            var anchorHinge = m_Anchor.AddComponent<HingeJoint2D>();
            anchorHinge.anchor = new Vector2(0f, halfSpacing);
            anchorHinge.connectedBody = null;
            anchorHinge.autoConfigureConnectedAnchor = true;

            // Create joints
            Rigidbody2D previousRb = anchorRb;
            bool isFirstJoint = true;

            for (int i = 0; i < m_ChainLength; i++)
            {
                GameObject joint = Instantiate(m_JointPrefab, transform);
                joint.name = $"ChainJoint_{i}";
                joint.transform.localPosition = new Vector3(0f, -i * m_JointSpacing, 0f);

                var hinge = joint.GetComponent<HingeJoint2D>();
                if (hinge == null)
                    hinge = joint.AddComponent<HingeJoint2D>();

                hinge.anchor = new Vector2(0f, halfSpacing);
                hinge.connectedBody = previousRb;

                if (isFirstJoint)
                {
                    // joint_0 → anchor: manual anchor, connectedAnchor = top of anchor in anchor local space
                    hinge.autoConfigureConnectedAnchor = false;
                    hinge.connectedAnchor = new Vector2(0f, halfSpacing);
                    isFirstJoint = false;
                }
                else
                {
                    // joint_1..N → previous joint: let physics auto-configure
                    hinge.autoConfigureConnectedAnchor = true;
                }

                previousRb = joint.GetComponent<Rigidbody2D>();
                m_Joints.Add(joint);
            }
        }

        public void Clear()
        {
            foreach (var joint in m_Joints)
            {
                if (joint == null) continue;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(joint);
                else
#endif
                    Destroy(joint);
            }
            m_Joints.Clear();

            if (m_Anchor != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(m_Anchor);
                else
#endif
                    Destroy(m_Anchor);
                m_Anchor = null;
            }

            // Clean up any leftover children not tracked
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child);
                else
#endif
                    Destroy(child);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EntityChain))]
    public class EntityChainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EntityChain chain = (EntityChain)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Build Chain"))
            {
                Undo.RegisterFullObjectHierarchyUndo(chain.gameObject, "Build Chain");
                chain.Setup();
                EditorUtility.SetDirty(chain.gameObject);
            }

            if (GUILayout.Button("Clear Chain"))
            {
                Undo.RegisterFullObjectHierarchyUndo(chain.gameObject, "Clear Chain");
                chain.Clear();
                EditorUtility.SetDirty(chain.gameObject);
            }
        }
    }
#endif
}
