using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Core
{
    public static class VgInput
    {
        public static InputSettings Settings
        {
            get
            {
                if (!m_Initialized)
                    Initialize();
                return m_Settings;
            }
        }

        public static InputBuffer Buffer
        {
            get { return m_InputBuffer ??= new InputBuffer(); }
        }

        public static void Initialize()
        {
            if (m_Initialized)
                return;

            m_Settings = InputSettings.Load();

            m_InputActions = new VgInputActions();
            m_InputActions.Enable();

            m_Keyboard = Keyboard.current;
            m_Mouse = Mouse.current;
            m_Gamepad = Gamepad.current;

            m_Initialized = true;

            CLogger.LogInfo("VgInput system initialized with New Input System.", LogTag.Input);
        }

        public static void Update()
        {
            if (!m_Initialized)
                Initialize();

            RefreshDevices();
            UpdateActions();
            UpdateAxes();
        }

        private static void RefreshDevices()
        {
            m_Keyboard ??= Keyboard.current;
            m_Mouse ??= Mouse.current;
            m_Gamepad ??= Gamepad.current;
        }

        public static bool GetButtonDownBuffered(InputAction action) =>
            GetButtonDown(action) || m_InputBuffer.HasRecentInput(action);

        public static bool GetButtonDown(InputAction action)
        {
            if (!m_Initialized)
                Initialize();

            var unityAction = GetUnityInputAction(action);
            if (unityAction?.WasPressedThisFrame() == true)
            {
                InputEvents.TriggerButtonPressed(action);
                m_InputBuffer.AddInput(action);
                return true;
            }
            return false;
        }

        public static bool GetButton(InputAction action)
        {
            if (!m_Initialized)
                Initialize();

            var unityAction = GetUnityInputAction(action);
            return unityAction?.IsPressed() == true;
        }

        public static bool GetButtonUp(InputAction action)
        {
            if (!m_Initialized)
                Initialize();

            var unityAction = GetUnityInputAction(action);
            if (unityAction != null && unityAction.WasReleasedThisFrame())
            {
                InputEvents.TriggerButtonReleased(action);
                return true;
            }
            return false;
        }

        public static float GetAxis(InputAxis axis)
        {
            if (!m_Initialized)
                Initialize();

            float value = GetAxisRaw(axis);
            return Mathf.Clamp(value, -1f, 1f);
        }

        public static float GetAxisRaw(InputAxis axis)
        {
            if (!m_Initialized)
                Initialize();

            return axis switch
            {
                InputAxis.LeftStickHorizontal => GetVector2Value(m_InputActions.Gameplay.Move).x,
                InputAxis.LeftStickVertical => GetVector2Value(m_InputActions.Gameplay.Move).y,
                InputAxis.RightStickHorizontal => GetVector2Value(
                    m_InputActions.Gameplay.RightStick
                ).x,
                InputAxis.RightStickVertical => GetVector2Value(
                    m_InputActions.Gameplay.RightStick
                ).y,
                InputAxis.LeftTrigger => GetFloatValue(m_InputActions.Gameplay.LeftTrigger),
                InputAxis.RightTrigger => GetFloatValue(m_InputActions.Gameplay.RightTrigger),
                InputAxis.MouseX => GetVector2Value(m_InputActions.Gameplay.Look).x,
                InputAxis.MouseY => GetVector2Value(m_InputActions.Gameplay.Look).y,
                InputAxis.MouseScrollWheel => GetFloatValue(m_InputActions.Gameplay.ScrollWheel),
                _ => 0f,
            };
        }

        public static Vector2 GetMovementVector()
        {
            if (!m_Initialized)
                Initialize();

            return GetVector2Value(m_InputActions.Gameplay.Move);
        }

        public static Vector2 GetMovementVectorNormalized()
        {
            Vector2 movement = GetMovementVector();
            if (movement.sqrMagnitude > 1f)
                movement.Normalize();
            return movement;
        }

        public static Vector2 GetLookVector()
        {
            float mouseX = GetAxis(InputAxis.MouseX);
            float mouseY = GetAxis(InputAxis.MouseY);

            mouseX *= m_Settings.mouseSensitivity;
            mouseY *= m_Settings.mouseSensitivity;

            if (m_Settings.invertMouseY)
                mouseY = -mouseY;

            return new Vector2(mouseX, mouseY);
        }

        public static Vector2 GetRightStickVector()
        {
            float horizontal = GetAxis(InputAxis.RightStickHorizontal);
            float vertical = GetAxis(InputAxis.RightStickVertical);

            horizontal *= m_Settings.gamepadSensitivity;
            vertical *= m_Settings.gamepadSensitivity;

            if (m_Settings.invertGamepadY)
                vertical = -vertical;

            return new Vector2(horizontal, vertical);
        }

        public static float GetMouseScrollWheel()
        {
            return GetAxis(InputAxis.MouseScrollWheel);
        }

        public static Vector3 GetMousePosition()
        {
            if (!m_Initialized)
                Initialize();

            return GetVector2Value(m_InputActions.Gameplay.MousePosition);
        }

        public static Vector3 GetMouseWorldPosition(Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;

            if (camera == null)
                return Vector3.zero;

            return camera.ScreenToWorldPoint(GetMousePosition());
        }

        public static bool AnyKeyDown()
        {
            if (m_Keyboard?.anyKey.wasPressedThisFrame == true)
                return true;
            if (
                m_Mouse != null
                && (
                    m_Mouse.leftButton.wasPressedThisFrame
                    || m_Mouse.rightButton.wasPressedThisFrame
                    || m_Mouse.middleButton.wasPressedThisFrame
                )
            )
                return true;
            if (m_Gamepad != null)
            {
                foreach (var button in m_Gamepad.allControls)
                {
                    if (button is ButtonControl btnCtrl && btnCtrl.wasPressedThisFrame)
                        return true;
                }
            }
            return false;
        }

        public static void SetMouseSensitivity(float sensitivity)
        {
            if (!m_Initialized)
                Initialize();

            m_Settings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
        }

        public static void SetGamepadSensitivity(float sensitivity)
        {
            if (!m_Initialized)
                Initialize();

            m_Settings.gamepadSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
        }

        public static void ToggleInvertMouseY()
        {
            if (!m_Initialized)
                Initialize();

            m_Settings.invertMouseY = !m_Settings.invertMouseY;
        }

        private static UnityEngine.InputSystem.InputAction GetUnityInputAction(InputAction action)
        {
            if (m_InputActions == null)
                return null;

            if (!m_ActionCache.TryGetValue(action, out var unityAction))
            {
                var actionName = action.ToString();
                var gameplayActions = m_InputActions.Gameplay;
                var actionProperty = gameplayActions
                    .GetType()
                    .GetProperty(
                        actionName,
                        System.Reflection.BindingFlags.Public
                            | System.Reflection.BindingFlags.Instance
                    );

                if (
                    actionProperty != null
                    && actionProperty.PropertyType == typeof(UnityEngine.InputSystem.InputAction)
                )
                {
                    unityAction =
                        actionProperty.GetValue(gameplayActions)
                        as UnityEngine.InputSystem.InputAction;
                    m_ActionCache[action] = unityAction;
                }
            }

            return unityAction;
        }

        private static Vector2 GetVector2Value(UnityEngine.InputSystem.InputAction action)
        {
            if (action == null)
                return Vector2.zero;

            return action.ReadValue<Vector2>();
        }

        private static float GetFloatValue(UnityEngine.InputSystem.InputAction action)
        {
            if (action == null)
                return 0f;

            return action.ReadValue<float>();
        }

        private static void UpdateActions()
        {
            foreach (var action in Enum.GetValues(typeof(InputAction)).Cast<InputAction>())
            {
                bool isHeld = GetButton(action);

                if (isHeld && m_HeldActions.Contains(action))
                {
                    InputEvents.TriggerButtonHeld(action);
                }
                else if (isHeld)
                {
                    m_HeldActions.Add(action);
                }
                else if (!isHeld && m_HeldActions.Contains(action))
                {
                    m_HeldActions.Remove(action);
                }
            }
        }

        private static void UpdateAxes()
        {
            foreach (var axis in Enum.GetValues(typeof(InputAxis)).Cast<InputAxis>())
            {
                float currentValue = GetAxis(axis);

                if (!m_PreviousAxisValues.ContainsKey(axis))
                {
                    m_PreviousAxisValues[axis] = currentValue;
                }

                if (Mathf.Abs(currentValue - m_PreviousAxisValues[axis]) > 0.01f)
                {
                    InputEvents.TriggerAxisChanged(axis, currentValue);
                    m_PreviousAxisValues[axis] = currentValue;
                }
            }
        }

        // csharpier-ignore-start
        private static VgInputActions                                                     m_InputActions       ;
        private static InputSettings                                                      m_Settings           ;
        private static bool                                                               m_Initialized        = false!;
        private static InputBuffer                                                        m_InputBuffer        = new();
        private static Dictionary<InputAxis, float>                                       m_PreviousAxisValues = new();
        private static HashSet<InputAction>                                               m_HeldActions        = new();
        private static Dictionary<InputAction, UnityEngine.InputSystem.InputAction>       m_ActionCache        = new();
        private static Keyboard                                                           m_Keyboard           ;
        private static Mouse                                                              m_Mouse              ;
        private static Gamepad                                                            m_Gamepad            ;
        // csharpier-ignore-end
    }
}
