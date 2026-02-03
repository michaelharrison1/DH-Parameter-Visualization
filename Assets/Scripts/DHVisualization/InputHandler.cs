using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using MagicLeap.Examples;

namespace DHVisualization
{
    /// <summary>
    /// Handles Magic Leap 2 controller input for navigating the visualization flow.
    /// Trigger advances to the next joint, Bumper goes back to the previous joint.
    /// Ignores trigger input when interacting with UI elements.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Tooltip("Reference to the visualization state controller")]
        public VisualizationStateController stateController;

        [Tooltip("Enable haptic feedback on button presses")]
        public bool enableHaptics = true;

        [Tooltip("Duration of haptic pulse in milliseconds")]
        [Range(10, 500)]
        public uint hapticDurationMs = 100;

        [Tooltip("Amplitude of haptic pulse (0-1)")]
        [Range(0f, 1f)]
        public float hapticAmplitude = 0.5f;

        private void OnEnable()
        {
            try
            {
                MagicLeapController.Instance.TriggerPressed += OnTriggerPressed;
                MagicLeapController.Instance.BumperPressed += OnBumperPressed;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InputHandler] Failed to subscribe to controller events: {e.Message}");
            }
        }

        private void OnDisable()
        {
            try
            {
                MagicLeapController.Instance.TriggerPressed -= OnTriggerPressed;
                MagicLeapController.Instance.BumperPressed -= OnBumperPressed;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InputHandler] Failed to unsubscribe from controller events: {e.Message}");
            }
        }
        
        /// <summary>
        /// Checks if the user is currently interacting with a UI element.
        /// </summary>
        /// <returns>True if pointing at or interacting with UI</returns>
        private bool IsInteractingWithUI()
        {
            // Check if EventSystem has a currently selected object
            if (EventSystem.current != null)
            {
                // Check if pointer is over a UI element
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return true;
                }
                
                // Check if there's a currently selected UI element
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when the trigger button is pressed.
        /// Advances to the next joint in the visualization.
        /// </summary>
        /// <param name="context">Input action callback context.</param>
        private void OnTriggerPressed(InputAction.CallbackContext context)
        {
            if (stateController == null)
            {
                Debug.LogWarning("[InputHandler] StateController is not assigned!");
                return;
            }
            
            // Don't advance if interacting with UI
            if (IsInteractingWithUI())
            {
                return;
            }

            bool advanced = stateController.TryAdvance();
            
            if (advanced && enableHaptics)
            {
                TriggerHapticFeedback();
            }
        }

        /// <summary>
        /// Called when the bumper button is pressed.
        /// Goes back to the previous joint in the visualization.
        /// </summary>
        /// <param name="context">Input action callback context.</param>
        private void OnBumperPressed(InputAction.CallbackContext context)
        {
            if (stateController == null)
            {
                Debug.LogWarning("[InputHandler] StateController is not assigned!");
                return;
            }

            bool wentBack = stateController.TryGoBack();
            
            if (wentBack && enableHaptics)
            {
                TriggerHapticFeedback();
            }
        }

        /// <summary>
        /// Triggers haptic feedback on the controller.
        /// </summary>
        private void TriggerHapticFeedback()
        {
            try
            {
                // Use Unity's XR haptic system for controller feedback
                var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
                if (device.isValid)
                {
                    UnityEngine.XR.HapticCapabilities capabilities;
                    if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                    {
                        device.SendHapticImpulse(0, hapticAmplitude, hapticDurationMs / 1000f);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InputHandler] Failed to trigger haptic feedback: {e.Message}");
            }
        }
    }
}
