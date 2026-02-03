using UnityEngine;
using UnityEngine.UI;

namespace DHVisualization
{
    /// <summary>
    /// Updates UI text elements based on the current visualization state.
    /// Listens to events from the VisualizationStateController to keep UI in sync.
    /// </summary>
    public class VisualizationUIController : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the state controller to subscribe to state changes")]
        public VisualizationStateController stateController;

        [Header("UI Text Elements")]
        [Tooltip("Displays 'Step 1', 'Step 2', etc.")]
        public Text titleText;

        [Tooltip("Displays 'Joint 0 Assignment', 'Joint 1 Assignment', etc.")]
        public Text contentText;

        [Tooltip("Displays instructions like 'Choose Z' or similar")]
        public Text instructionText;

        private void OnEnable()
        {
            if (stateController != null)
            {
                stateController.OnStateChanged += OnStateChanged;
                stateController.OnVisualizationComplete += OnVisualizationComplete;
            }
            else
            {
                Debug.LogWarning("[VisualizationUIController] StateController is not assigned. UI will not update.");
            }
        }

        private void OnDisable()
        {
            if (stateController != null)
            {
                stateController.OnStateChanged -= OnStateChanged;
                stateController.OnVisualizationComplete -= OnVisualizationComplete;
            }
        }

        /// <summary>
        /// Called when the visualization state changes.
        /// Updates all UI text elements to reflect the new state.
        /// </summary>
        /// <param name="newState">The new visualization state</param>
        /// <param name="jointIndex">The current joint index being assigned</param>
        private void OnStateChanged(VisualizationState newState, int jointIndex)
        {
            // Update title to display step number (joint 0 = step 1)
            if (titleText != null)
            {
                titleText.text = $"Step {jointIndex + 1}";
            }

            // Update content to display joint assignment info
            if (contentText != null)
            {
                contentText.text = $"Joint {jointIndex} Assignment";
            }

            // Update instruction based on current state
            if (instructionText != null)
            {
                instructionText.text = GetInstructionForState(newState);
            }
        }

        /// <summary>
        /// Called when visualization is complete.
        /// Updates UI to show completion status.
        /// </summary>
        private void OnVisualizationComplete()
        {
            if (titleText != null)
            {
                titleText.text = "Complete";
            }

            if (contentText != null)
            {
                contentText.text = "All joints assigned";
            }

            if (instructionText != null)
            {
                instructionText.text = string.Empty;
            }
        }

        /// <summary>
        /// Returns the appropriate instruction text for the given state.
        /// </summary>
        /// <param name="state">The current visualization state</param>
        /// <returns>Instruction text to display</returns>
        private string GetInstructionForState(VisualizationState state)
        {
            switch (state)
            {
                case VisualizationState.Initializing:
                    return "Initializing...";
                case VisualizationState.AssigningJoint0:
                case VisualizationState.AssigningJoint1:
                case VisualizationState.AssigningJoint2:
                case VisualizationState.AssigningJoint3:
                case VisualizationState.AssigningJoint4:
                case VisualizationState.AssigningJoint5:
                    return "Choose Z";
                case VisualizationState.Complete:
                    return string.Empty;
                default:
                    return "Choose Z";
            }
        }
    }
}
