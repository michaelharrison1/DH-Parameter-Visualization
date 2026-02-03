using System;
using System.Collections.Generic;
using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Manages all JointVisual instances for the robot.
    /// Spawns XYZAxis prefabs for each joint at startup, parents them to the correct TagAnchor,
    /// and provides methods for controlling visibility.
    /// </summary>
    public class JointVisualizerManager : MonoBehaviour
    {
        [Tooltip("Reference to the robot configuration ScriptableObject")]
        public RobotConfiguration robotConfig;

        [Tooltip("The XYZAxis prefab to instantiate for each joint")]
        public GameObject xyzAxisPrefab;

        [Tooltip("Reference to the TagAnchorManager to get parent anchors")]
        public TagAnchorManager tagAnchorManager;

        // Stores all spawned joint visuals
        private List<JointVisual> _allJointVisuals = new List<JointVisual>();

        // Maps JointID to its visual for quick lookup
        private Dictionary<int, JointVisual> _jointVisualsById = new Dictionary<int, JointVisual>();

        /// <summary>
        /// Whether the manager has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Initializes the joint visualization system by spawning XYZAxis prefabs
        /// for each joint defined in the robot configuration.
        /// </summary>
        public void Initialize()
        {
            if (robotConfig == null)
            {
                Debug.LogError("JointVisualizerManager: RobotConfiguration is not assigned. Cannot initialize.");
                return;
            }

            if (xyzAxisPrefab == null)
            {
                Debug.LogError("JointVisualizerManager: XYZAxis prefab is not assigned. Cannot initialize.");
                return;
            }

            if (tagAnchorManager == null)
            {
                Debug.LogError("JointVisualizerManager: TagAnchorManager is not assigned. Cannot initialize.");
                return;
            }

            if (!tagAnchorManager.IsInitialized)
            {
                Debug.LogWarning("JointVisualizerManager: TagAnchorManager is not initialized. Initializing it now.");
                tagAnchorManager.Initialize();
            }

            // Clear any existing visuals
            ClearVisuals();

            if (robotConfig.TagMappings == null || robotConfig.TagMappings.Count == 0)
            {
                Debug.LogWarning("JointVisualizerManager: RobotConfiguration has no TagMappings. No visuals will be created.");
                IsInitialized = true;
                return;
            }

            // Iterate through all tag mappings and create visuals for each joint
            foreach (var tagMapping in robotConfig.TagMappings)
            {
                if (tagMapping == null || tagMapping.Joints == null)
                {
                    continue;
                }

                // Get the TagAnchor for this AprilTag ID
                TagAnchor tagAnchor = tagAnchorManager.GetAnchor(tagMapping.AprilTagID);
                if (tagAnchor == null)
                {
                    Debug.LogWarning($"JointVisualizerManager: No TagAnchor found for AprilTag ID {tagMapping.AprilTagID}. Skipping joints for this tag.");
                    continue;
                }

                // Create a visual for each joint in this tag mapping
                foreach (var joint in tagMapping.Joints)
                {
                    if (joint == null)
                    {
                        continue;
                    }

                    // Check for duplicate JointID
                    if (_jointVisualsById.ContainsKey(joint.JointID))
                    {
                        Debug.LogWarning($"JointVisualizerManager: Duplicate JointID {joint.JointID} found. Skipping duplicate.");
                        continue;
                    }

                    // Instantiate the prefab
                    GameObject visualObject = Instantiate(xyzAxisPrefab);
                    visualObject.name = $"JointVisual_{joint.JointID}";

                    // Parent to the TagAnchor's transform
                    visualObject.transform.SetParent(tagAnchor.transform);

                    // Set local position and rotation from the joint's offsets
                    visualObject.transform.localPosition = joint.PositionOffset;
                    visualObject.transform.localRotation = Quaternion.Euler(joint.RotationOffset);

                    // Add or get the JointVisual component
                    JointVisual jointVisual = visualObject.GetComponent<JointVisual>();
                    if (jointVisual == null)
                    {
                        jointVisual = visualObject.AddComponent<JointVisual>();
                    }

                    // Set the JointID
                    jointVisual.JointID = joint.JointID;

                    // Add to the list and dictionary
                    _allJointVisuals.Add(jointVisual);
                    _jointVisualsById[joint.JointID] = jointVisual;

                    // Start hidden
                    jointVisual.Hide();
                }
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Shows all joint visuals up to and including the specified JointID.
        /// </summary>
        /// <param name="maxJointID">The maximum JointID to show (inclusive)</param>
        public void ShowJointsUpTo(int maxJointID)
        {
            foreach (var jointVisual in _allJointVisuals)
            {
                if (jointVisual == null)
                {
                    continue;
                }

                if (jointVisual.JointID <= maxJointID)
                {
                    jointVisual.Show();
                }
                else
                {
                    jointVisual.Hide();
                }
            }
        }

        /// <summary>
        /// Hides all joint visuals.
        /// </summary>
        public void HideAllJoints()
        {
            foreach (var jointVisual in _allJointVisuals)
            {
                if (jointVisual != null)
                {
                    jointVisual.Hide();
                }
            }
        }

        /// <summary>
        /// Shows all joint visuals.
        /// </summary>
        public void ShowAllJoints()
        {
            foreach (var jointVisual in _allJointVisuals)
            {
                if (jointVisual != null)
                {
                    jointVisual.Show();
                }
            }
        }

        /// <summary>
        /// Gets the JointVisual for a specific JointID.
        /// </summary>
        /// <param name="jointID">The JointID to look up</param>
        /// <returns>The JointVisual if found, null otherwise</returns>
        public JointVisual GetJointVisual(int jointID)
        {
            if (_jointVisualsById.TryGetValue(jointID, out JointVisual visual))
            {
                return visual;
            }

            Debug.LogWarning($"JointVisualizerManager: No visual found for JointID {jointID}");
            return null;
        }

        /// <summary>
        /// Clears all existing joint visuals and destroys their GameObjects.
        /// </summary>
        public void ClearVisuals()
        {
            foreach (var jointVisual in _allJointVisuals)
            {
                if (jointVisual != null && jointVisual.gameObject != null)
                {
                    Destroy(jointVisual.gameObject);
                }
            }
            _allJointVisuals.Clear();
            _jointVisualsById.Clear();
            IsInitialized = false;
        }

        private void OnDestroy()
        {
            ClearVisuals();
        }
    }
}
