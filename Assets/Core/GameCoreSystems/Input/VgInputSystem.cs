using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

namespace Core
{
    public class VgInputSystem : CoreModuleManagerBase<VgInputSystem>, ICoreModuleSystem
    {
        public string SystemName => "VgInputSystem";
        public Type[] Dependencies => Array.Empty<Type>();

        public void RegisterHooks(IGameCoreHookRegistry registry)
        {
            registry.OnSystemInit(async () =>
            {
                InputSystem.onDeviceChange += OnDeviceChange;
#if UNITY_EDITOR
                InitializeDebugData();
#endif
                await UniTask.CompletedTask;
            });

            registry.OnUpdate(Tick);

            registry.OnGameQuit(async () =>
            {
                InputSystem.onDeviceChange -= OnDeviceChange;
                await UniTask.CompletedTask;
            });
        }

        private void Tick()
        {
            VgInput.Update();

#if UNITY_EDITOR
            UpdateDebugDisplay();
#endif
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    CLogger.LogInfo($"Input device added: {device.displayName}", LogTag.Input);
                    break;
                case InputDeviceChange.Removed:
                    CLogger.LogInfo($"Input device removed: {device.displayName}", LogTag.Input);
                    break;
                case InputDeviceChange.Reconnected:
                    CLogger.LogInfo(
                        $"Input device reconnected: {device.displayName}",
                        LogTag.Input
                    );
                    break;
                case InputDeviceChange.Disconnected:
                    CLogger.LogInfo(
                        $"Input device disconnected: {device.displayName}",
                        LogTag.Input
                    );
                    break;
            }
        }

#if UNITY_EDITOR
        [BoxGroup("Settings")]
        [LabelText("Input Settings Asset")]
#endif
        public InputSettings inputSettings;

#if UNITY_EDITOR
        private void InitializeDebugData()
        {
            m_InputActionStates.Clear();
            m_InputAxisValues.Clear();

            foreach (InputAction action in Enum.GetValues(typeof(InputAction)))
            {
                m_InputActionStates[action] = new InputActionState();
            }

            foreach (InputAxis axis in Enum.GetValues(typeof(InputAxis)))
            {
                m_InputAxisValues[axis] = 0f;
            }
        }

        private void UpdateDebugDisplay()
        {
            if (!m_ShowRealTimeInput)
                return;

            try
            {
                foreach (InputAction action in Enum.GetValues(typeof(InputAction)))
                {
                    if (!m_InputActionStates.ContainsKey(action))
                    {
                        m_InputActionStates[action] = new InputActionState();
                    }

                    var state = m_InputActionStates[action];
                    state.isPressed = VgInput.GetButtonDown(action);
                    state.isHeld = VgInput.GetButton(action);
                    state.isReleased = VgInput.GetButtonUp(action);
                }

                foreach (InputAxis axis in Enum.GetValues(typeof(InputAxis)))
                {
                    if (!m_InputAxisValues.ContainsKey(axis))
                    {
                        m_InputAxisValues[axis] = 0f;
                    }

                    m_InputAxisValues[axis] = VgInput.GetAxis(axis);
                }

                m_MovementVector = VgInput.GetMovementVector();
                m_MovementVectorNormalized = VgInput.GetMovementVectorNormalized();
                m_LookVector = VgInput.GetLookVector();
                m_RightStickVector = VgInput.GetRightStickVector();
                m_MousePosition = VgInput.GetMousePosition();
                m_MouseScrollWheel = VgInput.GetMouseScrollWheel();

                if (Input.GetKeyDown(KeyCode.F1))
                {
                    CLogger.LogInfo(
                        $"Debug Input Status:\n"
                            + $"Movement Vector: {m_MovementVector}\n"
                            + $"Jump State: Pressed={m_InputActionStates[InputAction.Jump].isPressed}, Held={m_InputActionStates[InputAction.Jump].isHeld}\n"
                            + $"Show Real-Time Input: {m_ShowRealTimeInput}",
                        LogTag.Input
                    );
                }
            }
            catch (System.Exception e)
            {
                CLogger.LogError($"Error in UpdateDebugDisplay: {e.Message}", LogTag.Input);
            }
        }

        private Color GetScrollColor(float value)
        {
            if (Mathf.Abs(value) < 0.01f)
                return Color.gray;
            return value > 0 ? Color.green : Color.red;
        }

        [Serializable]
        public class InputActionState
        {
            [HorizontalGroup("State")]
            [LabelWidth(80)]
            [ShowInInspector, ReadOnly]
            public bool isPressed;

            [HorizontalGroup("State")]
            [LabelWidth(60)]
            [ShowInInspector, ReadOnly]
            public bool isHeld;

            [HorizontalGroup("State")]
            [LabelWidth(80)]
            [ShowInInspector, ReadOnly]
            public bool isReleased;
        }



        [BoxGroup("Debug Display")]
        [LabelText("Show Real-Time Input")]
        [ToggleLeft]
        [SerializeField]
        private bool m_ShowRealTimeInput = true;

        [BoxGroup("Debug Display/Input Actions")]
        [ShowInInspector, ReadOnly]
        [LabelText("Action States")]
        private Dictionary<InputAction, InputActionState> m_InputActionStates = new();

        [BoxGroup("Debug Display/Input Axes")]
        [ShowInInspector, ReadOnly]
        [LabelText("Axis Values")]
        private Dictionary<InputAxis, float> m_InputAxisValues = new();

        [BoxGroup("Debug Display/Composite Inputs")]
        [ShowInInspector, ReadOnly]
        [LabelText("Movement Vector")]
        private Vector2 m_MovementVector;

        [BoxGroup("Debug Display/Composite Inputs")]
        [ShowInInspector, ReadOnly]
        [LabelText("Movement (Normalized)")]
        private Vector2 m_MovementVectorNormalized;

        [BoxGroup("Debug Display/Composite Inputs")]
        [ShowInInspector, ReadOnly]
        [LabelText("Look Vector")]
        private Vector2 m_LookVector;

        [BoxGroup("Debug Display/Composite Inputs")]
        [ShowInInspector, ReadOnly]
        [LabelText("Right Stick Vector")]
        private Vector2 m_RightStickVector;

        [BoxGroup("Debug Display/Mouse")]
        [ShowInInspector, ReadOnly]
        [LabelText("Mouse Position")]
        private Vector3 m_MousePosition;

        [BoxGroup("Debug Display/Mouse")]
        [ShowInInspector, ReadOnly]
        [LabelText("Mouse Scroll Wheel")]
        [ProgressBar(-5, 5, ColorGetter = "GetScrollColor")]
        private float m_MouseScrollWheel;
#endif
    }
}
