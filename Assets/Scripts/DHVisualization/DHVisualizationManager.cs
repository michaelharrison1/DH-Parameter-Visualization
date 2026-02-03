using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using MagicLeap.OpenXR.Features.MarkerUnderstanding;

namespace DHVisualization
{
    /// <summary>
    /// Master manager that serves as the single entry point for the DH Visualization system.
    /// Initializes the AprilTag detector, wires up all other managers, and coordinates
    /// the flow of marker data to the visualization system.
    /// 
    /// SETUP: All child managers must be manually assigned in the Inspector.
    /// </summary>
    public class DHVisualizationManager : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Configuration")]
        [Tooltip("The robot configuration ScriptableObject containing tag mappings and joint definitions")]
        public RobotConfiguration robotConfig;

        [Tooltip("The XYZ axis visualization prefab to instantiate for each joint")]
        public GameObject xyzAxisPrefab;

        [Header("Deadband Settings")]
        [Tooltip("Position change threshold in meters below which updates are ignored")]
        public float positionDeadband = 0.02f;

        [Tooltip("Rotation change threshold in degrees below which updates are ignored")]
        public float rotationDeadband = 2.0f;

        [Header("Manager References (Required - Assign in Inspector)")]
        [Tooltip("Reference to the TagAnchorManager")]
        public TagAnchorManager tagAnchorManager;

        [Tooltip("Reference to the JointVisualizerManager")]
        public JointVisualizerManager jointVisualizerManager;

        [Tooltip("Reference to the VisualizationStateController")]
        public VisualizationStateController stateController;

        [Tooltip("Reference to the InputHandler")]
        public InputHandler inputHandler;

        [Tooltip("Reference to the VisualizationUIController")]
        public VisualizationUIController uiController;

        #endregion

        #region Private Fields

        private MagicLeapMarkerUnderstandingFeature markerFeature;
        private MarkerDetectorSettings detectorSettings;
        private bool isInitialized = false;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeSystem();
        }

        private void Update()
        {
            if (!isInitialized || markerFeature == null)
            {
                return;
            }

            markerFeature.UpdateMarkerDetectors();

            foreach (var markerDetector in markerFeature.MarkerDetectors)
            {
                ProcessMarkerDetectorData(markerDetector);
            }
        }

        private void OnDestroy()
        {
            if (markerFeature != null)
            {
                markerFeature.DestroyAllMarkerDetectors();
            }

            if (stateController != null)
            {
                stateController.OnStateChanged -= OnVisualizationStateChanged;
            }
        }

        #endregion

        #region Initialization

        private void InitializeSystem()
        {
            // Validate all required references
            if (!ValidateReferences())
            {
                Debug.LogError("[DHVisualizationManager] Missing required references. Check Inspector assignments.");
                return;
            }

            // Step 1: Initialize marker feature
            if (!InitializeMarkerFeature())
            {
                Debug.LogError("[DHVisualizationManager] Failed to initialize marker feature.");
                return;
            }

            // Step 2: Create marker detector
            if (!ConfigureMarkerDetector())
            {
                Debug.LogError("[DHVisualizationManager] Failed to configure marker detector.");
                return;
            }

            // Step 3: Initialize TagAnchorManager
            tagAnchorManager.robotConfig = robotConfig;
            tagAnchorManager.DefaultPositionDeadband = positionDeadband;
            tagAnchorManager.DefaultRotationDeadband = rotationDeadband;
            tagAnchorManager.Initialize();

            // Step 4: Initialize JointVisualizerManager
            jointVisualizerManager.robotConfig = robotConfig;
            jointVisualizerManager.xyzAxisPrefab = xyzAxisPrefab;
            jointVisualizerManager.tagAnchorManager = tagAnchorManager;
            jointVisualizerManager.Initialize();

            // Step 5: Initialize StateController
            stateController.robotConfig = robotConfig;
            stateController.Initialize();

            // Step 6: Wire up InputHandler
            inputHandler.stateController = stateController;

            // Step 7: Wire up UIController and force it to re-subscribe
            uiController.stateController = stateController;
            // Force re-subscription by toggling enabled
            uiController.enabled = false;
            uiController.enabled = true;

            // Step 8: Subscribe to state changes
            stateController.OnStateChanged += OnVisualizationStateChanged;

            // Step 9: Start visualization
            stateController.StartVisualization();
            
            // Step 9b: Explicitly set initial joint visibility to ensure correct state
            // This guarantees joint 0 is shown even before any state events fire
            jointVisualizerManager.ShowJointsUpTo(stateController.CurrentJointIndex);

            // Step 10: Force initial UI update (overwrite any stale text)
            ForceInitialUIUpdate();

            isInitialized = true;
        }

        private bool ValidateReferences()
        {
            bool valid = true;

            if (robotConfig == null)
            {
                Debug.LogError("[DHVisualizationManager] RobotConfiguration is not assigned!");
                valid = false;
            }

            if (xyzAxisPrefab == null)
            {
                Debug.LogError("[DHVisualizationManager] XYZ Axis Prefab is not assigned!");
                valid = false;
            }

            if (tagAnchorManager == null)
            {
                Debug.LogError("[DHVisualizationManager] TagAnchorManager is not assigned!");
                valid = false;
            }

            if (jointVisualizerManager == null)
            {
                Debug.LogError("[DHVisualizationManager] JointVisualizerManager is not assigned!");
                valid = false;
            }

            if (stateController == null)
            {
                Debug.LogError("[DHVisualizationManager] VisualizationStateController is not assigned!");
                valid = false;
            }

            if (inputHandler == null)
            {
                Debug.LogError("[DHVisualizationManager] InputHandler is not assigned!");
                valid = false;
            }

            if (uiController == null)
            {
                Debug.LogError("[DHVisualizationManager] VisualizationUIController is not assigned!");
                valid = false;
            }

            return valid;
        }

        private bool InitializeMarkerFeature()
        {
            markerFeature = OpenXRSettings.Instance.GetFeature<MagicLeapMarkerUnderstandingFeature>();

            if (markerFeature == null)
            {
                Debug.LogError("[DHVisualizationManager] MagicLeapMarkerUnderstandingFeature not found.");
                return false;
            }

            return true;
        }

        private bool ConfigureMarkerDetector()
        {
            detectorSettings = new MarkerDetectorSettings();
            detectorSettings.MarkerType = MarkerType.AprilTag;
            detectorSettings.AprilTagSettings.AprilTagType = AprilTagType.Dictionary_36H11;

            float tagSize = 0.1f;
            if (robotConfig.TagMappings != null && robotConfig.TagMappings.Count > 0)
            {
                tagSize = robotConfig.TagMappings[0].TagSizeMeters;
            }

            detectorSettings.AprilTagSettings.AprilTagLength = tagSize;
            detectorSettings.AprilTagSettings.EstimateAprilTagLength = false;
            detectorSettings.MarkerDetectorProfile = MarkerDetectorProfile.Default;

            try
            {
                markerFeature.CreateMarkerDetector(detectorSettings);
                    return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DHVisualizationManager] Failed to create detector: {e.Message}");
                return false;
            }
        }

        private void ForceInitialUIUpdate()
        {
            if (uiController.titleText != null)
            {
                uiController.titleText.text = $"Step {stateController.CurrentJointIndex + 1}";
            }
            if (uiController.contentText != null)
            {
                uiController.contentText.text = $"Joint {stateController.CurrentJointIndex} Assignment";
            }
            if (uiController.instructionText != null)
            {
                uiController.instructionText.text = "Choose Z";
            }
        }

        #endregion

        #region Marker Processing

        private void ProcessMarkerDetectorData(MarkerDetector markerDetector)
        {
            if (markerDetector?.Data == null) return;

            foreach (var markerData in markerDetector.Data)
            {
                if (markerData.MarkerPose == null) continue;

                int aprilTagId = (int)markerData.MarkerNumber;
                var pose = markerData.MarkerPose.Value;

                tagAnchorManager.UpdateAnchorPose(aprilTagId, pose.position, pose.rotation);
            }
        }

        #endregion

        #region Event Handlers

        private void OnVisualizationStateChanged(VisualizationState newState, int jointIndex)
        {
            jointVisualizerManager?.ShowJointsUpTo(jointIndex);
        }

        #endregion
    }
}
