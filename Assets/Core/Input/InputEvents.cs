using System;
using UnityEngine;

namespace Core
{
    public class InputEventArgs : EventArgs
    {
        public InputAction Action { get; }
        public float Timestamp { get; }

        public InputEventArgs(InputAction action)
        {
            Action = action;
            Timestamp = Time.time;
        }
    }

    public class AxisEventArgs : EventArgs
    {
        public InputAxis Axis { get; }
        public float Value { get; }
        public float Timestamp { get; }

        public AxisEventArgs(InputAxis axis, float value)
        {
            Axis = axis;
            Value = value;
            Timestamp = Time.time;
        }
    }

    public static class InputEvents
    {
        public static event EventHandler<InputEventArgs> OnButtonPressed;

        public static event EventHandler<InputEventArgs> OnButtonHeld;

        public static event EventHandler<InputEventArgs> OnButtonReleased;
        public static event EventHandler<AxisEventArgs> OnAxisChanged;

        internal static void TriggerButtonPressed(InputAction action)
        {
            OnButtonPressed?.Invoke(null, new InputEventArgs(action));
        }

        internal static void TriggerButtonHeld(InputAction action)
        {
            OnButtonHeld?.Invoke(null, new InputEventArgs(action));
        }

        internal static void TriggerButtonReleased(InputAction action)
        {
            OnButtonReleased?.Invoke(null, new InputEventArgs(action));
        }

        internal static void TriggerAxisChanged(InputAxis axis, float value)
        {
            OnAxisChanged?.Invoke(null, new AxisEventArgs(axis, value));
        }

        public static void ClearAllEvents()
        {
            OnButtonPressed = null;
            OnButtonHeld = null;
            OnButtonReleased = null;
            OnAxisChanged = null;
        }

        public static void SubscribeToAction(InputAction action, Action callback)
        {
            OnButtonPressed += (sender, args) =>
            {
                if (args.Action == action)
                    callback?.Invoke();
            };
        }

        public static void SubscribeToAxis(InputAxis axis, Action<float> callback)
        {
            OnAxisChanged += (sender, args) =>
            {
                if (args.Axis == axis)
                    callback?.Invoke(args.Value);
            };
        }
    }

    public class InputBuffer
    {
        public InputBuffer(float bufferTime = 0.5f)
        {
            this.m_BufferTime = bufferTime;
        }

        public void AddInput(InputAction action)
        {
            m_Buffer.Add(new InputRecord { action = action, timestamp = Time.time });

            CleanOldInputs();
        }

        public bool HasSequence(params InputAction[] sequence)
        {
            if (sequence.Length == 0 || m_Buffer.Count < sequence.Length)
                return false;

            CleanOldInputs();

            int sequenceIndex = 0;
            for (int i = m_Buffer.Count - 1; i >= 0 && sequenceIndex < sequence.Length; i--)
            {
                if (m_Buffer[i].action == sequence[sequence.Length - 1 - sequenceIndex])
                {
                    sequenceIndex++;
                }
            }

            return sequenceIndex == sequence.Length;
        }

        public bool HasRecentInput(InputAction action, float withinTime = 0.2f)
        {
            CleanOldInputs();

            for (int i = m_Buffer.Count - 1; i >= 0; i--)
            {
                if (m_Buffer[i].action == action && Time.time - m_Buffer[i].timestamp <= withinTime)
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            m_Buffer.Clear();
        }

        private void CleanOldInputs()
        {
            float currentTime = Time.time;
            m_Buffer.RemoveAll(record => currentTime - record.timestamp > m_BufferTime);
        }

        public int Count => m_Buffer.Count;

        private struct InputRecord
        {
            public InputAction action;
            public float timestamp;
        }

        private readonly System.Collections.Generic.List<InputRecord> m_Buffer = new();
        private readonly float m_BufferTime;
    }
}
