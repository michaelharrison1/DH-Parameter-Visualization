using System;
using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Manages the state machine for the DH parameter visualization flow.
    /// Controls transitions between joint assignment states and fires events on state changes.
    /// </summary>
    public class VisualizationStateController : MonoBehaviour
    {
        [Tooltip("Robot configuration containing joint definitions")]
        public RobotConfiguration robotConfig;

        private VisualizationState currentState = VisualizationState.Initializing;
        private int currentJointIndex = -1;
        private int totalJoints = 0;

        /// <summary>
        /// Read-only access to the current visualization state.
        /// </summary>
        public VisualizationState CurrentState => currentState;

        /// <summary>
        /// Read-only access to the current joint index being assigned.
        /// Returns -1 if not yet started or in Initializing state.
        /// </summary>
        public int CurrentJointIndex => currentJointIndex;

        /// <summary>
        /// Read-only access to the total number of joints in the configuration.
        /// </summary>
        public int TotalJoints => totalJoints;

        /// <summary>
        /// Event fired when the visualization state changes.
        /// Parameters: new state, current joint index.
        /// </summary>
        public event Action<VisualizationState, int> OnStateChanged;

        /// <summary>
        /// Event fired when visualization reaches the Complete state.
        /// </summary>
        public event Action OnVisualizationComplete;

        /// <summary>
        /// Initializes the state controller with the robot configuration.
        /// Must be called before starting visualization.
        /// </summary>
        public void Initialize()
        {
            if (robotConfig == null)
            {
                Debug.LogError("[VisualizationStateController] RobotConfiguration is not assigned!");
                return;
            }

            totalJoints = robotConfig.GetTotalJointCount();
            currentState = VisualizationState.Initializing;
            currentJointIndex = -1;
        }

        /// <summary>
        /// Starts the visualization flow by transitioning to AssigningJoint0.
        /// </summary>
        public void StartVisualization()
        {
            if (totalJoints <= 0)
            {
                Debug.LogWarning("[VisualizationStateController] No joints configured. Cannot start visualization.");
                currentState = VisualizationState.Complete;
                OnVisualizationComplete?.Invoke();
                return;
            }

            currentJointIndex = 0;
            currentState = GetStateForJointIndex(currentJointIndex);
            
            OnStateChanged?.Invoke(currentState, currentJointIndex);
        }

        /// <summary>
        /// Attempts to advance to the next joint.
        /// </summary>
        /// <returns>True if successfully advanced to next joint, false if at end or cannot advance.</returns>
        public bool TryAdvance()
        {
            // Cannot advance if already complete
            if (currentState == VisualizationState.Complete)
            {
                return false;
            }

            // Cannot advance if not started
            if (currentState == VisualizationState.Initializing)
            {
                return false;
            }

            // Increment joint index
            currentJointIndex++;

            // Check if we've completed all joints
            if (currentJointIndex >= totalJoints)
            {
                currentState = VisualizationState.Complete;
                OnStateChanged?.Invoke(currentState, currentJointIndex);
                OnVisualizationComplete?.Invoke();
                return false;
            }

            // Transition to next joint state
            currentState = GetStateForJointIndex(currentJointIndex);
            OnStateChanged?.Invoke(currentState, currentJointIndex);
            return true;
        }

        /// <summary>
        /// Attempts to go back to the previous joint.
        /// </summary>
        /// <returns>True if successfully went back, false if at beginning or cannot go back.</returns>
        public bool TryGoBack()
        {
            // Cannot go back if in Initializing state
            if (currentState == VisualizationState.Initializing)
            {
                return false;
            }

            // Cannot go back before Joint 0
            if (currentJointIndex <= 0)
            {
                return false;
            }

            // If we were complete, go back to the last joint
            if (currentState == VisualizationState.Complete)
            {
                currentJointIndex = totalJoints - 1;
                currentState = GetStateForJointIndex(currentJointIndex);
                OnStateChanged?.Invoke(currentState, currentJointIndex);
                return true;
            }

            // Decrement joint index
            currentJointIndex--;
            currentState = GetStateForJointIndex(currentJointIndex);
            OnStateChanged?.Invoke(currentState, currentJointIndex);
            return true;
        }

        /// <summary>
        /// Maps a joint index to the corresponding VisualizationState.
        /// </summary>
        /// <param name="jointIndex">The joint index (0-based).</param>
        /// <returns>The corresponding VisualizationState for the joint.</returns>
        private VisualizationState GetStateForJointIndex(int jointIndex)
        {
            return jointIndex switch
            {
                0 => VisualizationState.AssigningJoint0,
                1 => VisualizationState.AssigningJoint1,
                2 => VisualizationState.AssigningJoint2,
                3 => VisualizationState.AssigningJoint3,
                4 => VisualizationState.AssigningJoint4,
                5 => VisualizationState.AssigningJoint5,
                6 => VisualizationState.AssigningJoint6,
                7 => VisualizationState.AssigningJoint7,
                8 => VisualizationState.AssigningJoint8,
                9 => VisualizationState.AssigningJoint9,
                _ => VisualizationState.Complete // For joints beyond our enum, treat as complete
            };
        }
    }
}
