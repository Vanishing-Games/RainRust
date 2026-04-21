using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Core
{
    [ShowOdinSerializedPropertiesInInspector]
    public class VgSerializedScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.m_SerializationData);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.m_SerializationData);
        }

        [SerializeField, HideInInspector]
        private SerializationData m_SerializationData;
    }
}
