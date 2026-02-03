using System;
using System.Collections.Generic;
using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Manages all TagAnchor GameObjects for tracked AprilTags.
    /// Creates persistent anchors for each unique AprilTag ID defined in the robot configuration.
    /// </summary>
    public class TagAnchorManager : MonoBehaviour
    {
        [Tooltip("Reference to the robot configuration ScriptableObject")]
        public RobotConfiguration robotConfig;

        [Tooltip("Default position deadband in meters applied to all anchors")]
        public float DefaultPositionDeadband = 0.02f;

        [Tooltip("Default rotation deadband in degrees applied to all anchors")]
        public float DefaultRotationDeadband = 2.0f;

        // Dictionary mapping AprilTag IDs to their anchor GameObjects
        private Dictionary<int, TagAnchor> _tagAnchors = new Dictionary<int, TagAnchor>();

        /// <summary>
        /// Read-only access to the dictionary of all tag anchors.
        /// </summary>
        public IReadOnlyDictionary<int, TagAnchor> TagAnchors => _tagAnchors;

        /// <summary>
        /// Whether the manager has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Initializes the anchor system by creating TagAnchor GameObjects for each unique
        /// AprilTag ID defined in the robot configuration.
        /// </summary>
        public void Initialize()
        {
            if (robotConfig == null)
            {
                Debug.LogError("TagAnchorManager: RobotConfiguration is not assigned. Cannot initialize.");
                return;
            }

            if (robotConfig.TagMappings == null || robotConfig.TagMappings.Count == 0)
            {
                Debug.LogWarning("TagAnchorManager: RobotConfiguration has no TagMappings. No anchors will be created.");
                IsInitialized = true;
                return;
            }

            // Clear any existing anchors
            ClearAnchors();

            // Collect unique AprilTag IDs
            HashSet<int> uniqueTagIds = new HashSet<int>();
            foreach (var tagMapping in robotConfig.TagMappings)
            {
                if (tagMapping != null)
                {
                    uniqueTagIds.Add(tagMapping.AprilTagID);
                }
            }

            // Create an anchor for each unique tag ID
            foreach (int tagId in uniqueTagIds)
            {
                CreateAnchor(tagId);
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Creates a new TagAnchor for the specified AprilTag ID.
        /// </summary>
        /// <param name="aprilTagId">The AprilTag ID to create an anchor for</param>
        /// <returns>The created TagAnchor, or null if creation failed</returns>
        private TagAnchor CreateAnchor(int aprilTagId)
        {
            // Check if anchor already exists
            if (_tagAnchors.ContainsKey(aprilTagId))
            {
                Debug.LogWarning($"TagAnchorManager: Anchor for tag {aprilTagId} already exists.");
                return _tagAnchors[aprilTagId];
            }

            // Create a new empty GameObject
            GameObject anchorObject = new GameObject($"TagAnchor_{aprilTagId}");
            anchorObject.transform.SetParent(this.transform);

            // Add PoseStabilizer component and configure deadbands
            PoseStabilizer poseStabilizer = anchorObject.AddComponent<PoseStabilizer>();
            poseStabilizer.PositionDeadbandMeters = DefaultPositionDeadband;
            poseStabilizer.RotationDeadbandDegrees = DefaultRotationDeadband;

            // Add TagAnchor component and configure
            TagAnchor tagAnchor = anchorObject.AddComponent<TagAnchor>();
            tagAnchor.AprilTagID = aprilTagId;

            // Store in dictionary
            _tagAnchors[aprilTagId] = tagAnchor;

            return tagAnchor;
        }

        /// <summary>
        /// Gets the TagAnchor for the specified AprilTag ID.
        /// </summary>
        /// <param name="aprilTagId">The AprilTag ID to look up</param>
        /// <returns>The TagAnchor if found, null otherwise</returns>
        public TagAnchor GetAnchor(int aprilTagId)
        {
            if (_tagAnchors.TryGetValue(aprilTagId, out TagAnchor anchor))
            {
                return anchor;
            }

            Debug.LogWarning($"TagAnchorManager: No anchor found for AprilTag ID {aprilTagId}");
            return null;
        }

        /// <summary>
        /// Updates the pose of an anchor for the specified AprilTag ID.
        /// The update will only be applied if it exceeds the deadband thresholds.
        /// </summary>
        /// <param name="aprilTagId">The AprilTag ID to update</param>
        /// <param name="position">The new position from tracking</param>
        /// <param name="rotation">The new rotation from tracking</param>
        public void UpdateAnchorPose(int aprilTagId, Vector3 position, Quaternion rotation)
        {
            if (!_tagAnchors.TryGetValue(aprilTagId, out TagAnchor anchor))
            {
                Debug.LogWarning($"TagAnchorManager: Cannot update pose - no anchor found for AprilTag ID {aprilTagId}");
                return;
            }

            if (anchor == null)
            {
                Debug.LogError($"TagAnchorManager: Anchor for AprilTag ID {aprilTagId} is null in dictionary.");
                _tagAnchors.Remove(aprilTagId);
                return;
            }

            // Update the anchor's pose
            anchor.UpdateFromTracking(position, rotation);
        }

        /// <summary>
        /// Clears all existing anchors and destroys their GameObjects.
        /// </summary>
        public void ClearAnchors()
        {
            foreach (var kvp in _tagAnchors)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _tagAnchors.Clear();
            IsInitialized = false;
        }

        private void OnDestroy()
        {
            ClearAnchors();
        }
    }
}
