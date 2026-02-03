using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Represents a persistent anchor for a single tracked AprilTag.
    /// Maintains its position when tracking is lost by simply not receiving updates.
    /// Uses PoseStabilizer to filter out jitter and small movements.
    /// Children (joint visuals) are hidden until the tag is first tracked.
    /// </summary>
    public class TagAnchor : MonoBehaviour
    {
        [Tooltip("The AprilTag 36h11 marker ID this anchor tracks")]
        public int AprilTagID;

        // Reference to the PoseStabilizer component
        private PoseStabilizer _poseStabilizer;
        
        // Track if we've shown children after first tracking
        private bool _childrenVisible = false;

        /// <summary>
        /// The PoseStabilizer component attached to this anchor.
        /// </summary>
        public PoseStabilizer PoseStabilizer => _poseStabilizer;

        /// <summary>
        /// Whether this anchor has received at least one tracking update.
        /// </summary>
        public bool HasReceivedTracking => _poseStabilizer != null && _poseStabilizer.IsInitialized;

        private void Awake()
        {
            // PoseStabilizer is always added by TagAnchorManager before TagAnchor
            _poseStabilizer = GetComponent<PoseStabilizer>();
        }
        
        private void Start()
        {
            // Hide all children until we receive tracking
            SetChildrenActive(false);
        }
        
        /// <summary>
        /// Sets the active state of all child objects.
        /// When activating, respects each JointVisual's shouldBeVisible state.
        /// </summary>
        private void SetChildrenActive(bool active)
        {
            foreach (Transform child in transform)
            {
                if (active)
                {
                    // When activating, check if the child is a JointVisual
                    // and respect its visibility state
                    var jointVisual = child.GetComponent<JointVisual>();
                    if (jointVisual != null)
                    {
                        // Let JointVisual decide if it should be visible
                        // by calling its Show method which checks _shouldBeVisible
                        jointVisual.RefreshVisibility();
                    }
                    else
                    {
                        // Non-JointVisual children just get activated
                        child.gameObject.SetActive(true);
                    }
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
            _childrenVisible = active;
        }

        /// <summary>
        /// Updates the anchor's pose from raw tracking data.
        /// The pose will only be applied if it exceeds the PoseStabilizer's deadband thresholds.
        /// When tracking is lost, this method simply won't be called, and the anchor
        /// automatically persists at its last known position.
        /// On first tracking, children become visible.
        /// </summary>
        /// <param name="rawPosition">The raw position from the tracking system</param>
        /// <param name="rawRotation">The raw rotation from the tracking system</param>
        /// <returns>True if the pose was actually updated, false if within deadband</returns>
        public bool UpdateFromTracking(Vector3 rawPosition, Quaternion rawRotation)
        {
            if (_poseStabilizer == null)
            {
                Debug.LogError($"TagAnchor {AprilTagID}: PoseStabilizer is null. Cannot update pose.");
                return false;
            }

            // Try to update through the stabilizer
            bool poseUpdated = _poseStabilizer.TryUpdatePose(rawPosition, rawRotation);

            if (poseUpdated)
            {
                // Apply the stabilized pose to this transform
                transform.position = _poseStabilizer.StabilizedPosition;
                transform.rotation = _poseStabilizer.StabilizedRotation;
                
                // Show children on first successful tracking
                if (!_childrenVisible)
                {
                    SetChildrenActive(true);
                }
            }
            // If poseUpdated is false, we do nothing - the anchor stays at its cached position

            return poseUpdated;
        }
    }
}
