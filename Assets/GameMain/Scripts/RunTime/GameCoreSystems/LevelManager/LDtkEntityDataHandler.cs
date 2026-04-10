using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LDtkUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameMain.RunTime
{
    public abstract class LDtkEntityDataHandler : MonoBehaviour
    {
        [Serializable]
        public class DataRecord
        {
            public string Identifier;
            public LDtkFieldType Type;
            public bool IsArray;

            public int IntValue;
            public float FloatValue;
            public bool BoolValue;
            public string StringValue;
            public Color ColorValue;
            public Vector2 VectorValue;
            public LDtkReferenceToAnEntityInstance EntityRef;

            public List<int> IntArray = new();
            public List<float> FloatArray = new();
            public List<bool> BoolArray = new();
            public List<string> StringArray = new();
            public List<Color> ColorArray = new();
            public List<Vector2> VectorArray = new();
            public List<LDtkReferenceToAnEntityInstance> EntityRefArray = new();
        }

        private void Start()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            foreach (var record in m_Records)
            {
                ApplyRecord(record);
            }
        }

        private void ApplyRecord(DataRecord record)
        {
            string key = record.Identifier.StartsWith("_")
                ? record.Identifier.Substring(1)
                : record.Identifier;

            if (record.IsArray)
            {
                DispatchArrayField(key, record);
            }
            else
            {
                DispatchSingleField(key, record);
            }
        }

        private void DispatchSingleField(string key, DataRecord record)
        {
            switch (record.Type)
            {
                case LDtkFieldType.Int:
                    OnSetInt(key, record.IntValue);
                    break;
                case LDtkFieldType.Float:
                    OnSetFloat(key, record.FloatValue);
                    break;
                case LDtkFieldType.Bool:
                    OnSetBool(key, record.BoolValue);
                    break;
                case LDtkFieldType.String:
                case LDtkFieldType.Multiline:
                case LDtkFieldType.FilePath:
                    OnSetString(key, record.StringValue);
                    break;
                case LDtkFieldType.Color:
                    OnSetColor(key, record.ColorValue);
                    break;
                case LDtkFieldType.Enum:
                    OnSetEnum(key, record.StringValue);
                    break;
                case LDtkFieldType.Point:
                    OnSetPoint(key, record.VectorValue);
                    break;
                case LDtkFieldType.EntityRef:
                    OnSetEntity(key, record.EntityRef?.GetEntity()?.gameObject);
                    break;
            }
        }

        private void DispatchArrayField(string key, DataRecord record)
        {
            switch (record.Type)
            {
                case LDtkFieldType.Int:
                    OnSetIntArray(key, record.IntArray.ToArray());
                    break;
                case LDtkFieldType.Float:
                    OnSetFloatArray(key, record.FloatArray.ToArray());
                    break;
                case LDtkFieldType.Bool:
                    OnSetBoolArray(key, record.BoolArray.ToArray());
                    break;
                case LDtkFieldType.String:
                case LDtkFieldType.Multiline:
                case LDtkFieldType.FilePath:
                    OnSetStringArray(key, record.StringArray.ToArray());
                    break;
                case LDtkFieldType.Color:
                    OnSetColorArray(key, record.ColorArray.ToArray());
                    break;
                case LDtkFieldType.Enum:
                    OnSetEnumArray(key, record.StringArray.ToArray());
                    break;
                case LDtkFieldType.Point:
                    OnSetPointArray(key, record.VectorArray.ToArray());
                    break;
                case LDtkFieldType.EntityRef:
                    var gos = record
                        .EntityRefArray.Select(r => r?.GetEntity()?.gameObject)
                        .ToArray();
                    OnSetEntityArray(key, gos);
                    break;
            }
        }

        protected virtual void OnSetInt(string key, int value) { }

        protected virtual void OnSetFloat(string key, float value) { }

        protected virtual void OnSetBool(string key, bool value) { }

        protected virtual void OnSetString(string key, string value) { }

        protected virtual void OnSetColor(string key, Color value) { }

        protected virtual void OnSetEnum(string key, string enumValue) { }

        protected virtual void OnSetPoint(string key, Vector2 value) { }

        protected virtual void OnSetEntity(string key, GameObject entity) { }

        protected virtual void OnSetIntArray(string key, int[] values) { }

        protected virtual void OnSetFloatArray(string key, float[] values) { }

        protected virtual void OnSetBoolArray(string key, bool[] values) { }

        protected virtual void OnSetStringArray(string key, string[] values) { }

        protected virtual void OnSetColorArray(string key, Color[] values) { }

        protected virtual void OnSetEnumArray(string key, string[] enumValues) { }

        protected virtual void OnSetPointArray(string key, Vector2[] values) { }

        protected virtual void OnSetEntityArray(string key, GameObject[] entities) { }

        public void ClearRecords() => m_Records.Clear();

        public void AddRecord(DataRecord record) => m_Records.Add(record);

        public List<DataRecord> Records => m_Records;

        [SerializeField]
        [ShowInInspector]
        [Sirenix.OdinInspector.ReadOnly]
        private List<DataRecord> m_Records = new();
    }
}
