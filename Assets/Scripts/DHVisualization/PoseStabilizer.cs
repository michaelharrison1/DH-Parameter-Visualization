using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Filters incoming pose updates using deadband thresholds to reduce jitter.
    /// Only updates the cached pose when movement exceeds the configured thresholds.
    /// </summary>
    public class PoseStabilizer : MonoBehaviour
    {
        [Tooltip("Minimum position change in meters required to trigger an update")]
        public float PositionDeadbandMeters = 0.02f;

        [Tooltip("Minimum rotation change in degrees required to trigger an update")]
        public float RotationDeadbandDegrees = 2.0f;

        // Cached pose values
        private Vector3 _cachedPosition = Vector3.zero;
        private Quaternion _cachedRotation = Quaternion.identity;
        private bool _isInitialized = false;

        /// <summary>
        /// The current stabilized position after deadband filtering.
        /// </summary>
        public Vector3 StabilizedPosition => _cachedPosition;

        /// <summary>
        /// The current stabilized rotation after deadband filtering.
        /// </summary>
        public Quaternion StabilizedRotation => _cachedRotation;

        /// <summary>
        /// Whether the stabilizer has received at least one pose update.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Attempts to update the cached pose if the new pose exceeds deadband thresholds.
        /// </summary>
        /// <param name="newPosition">The new position to evaluate</param>
        /// <param name="newRotation">The new rotation to evaluate</param>
        /// <returns>True if the pose was updated, false if within deadband thresholds</returns>
        public bool TryUpdatePose(Vector3 newPosition, Quaternion newRotation)
        {
            // If not initialized, force the first update
            if (!_isInitialized)
            {
                ForceUpdatePose(newPosition, newRotation);
                return true;
            }

            // Calculate deltas from cached pose
            float positionDelta = Vector3.Distance(_cachedPosition, newPosition);
            float rotationDelta = Quaternion.Angle(_cachedRotation, newRotation);

            // Check if either delta exceeds its threshold
            bool positionExceedsThreshold = positionDelta >= PositionDeadbandMeters;
            bool rotationExceedsThreshold = rotationDelta >= RotationDeadbandDegrees;

            if (positionExceedsThreshold || rotationExceedsThreshold)
            {
                // Update cached values
                _cachedPosition = newPosition;
                _cachedRotation = newRotation;
                return true;
            }

            // Both deltas are within thresholds - no update needed
            return false;
        }

        /// <summary>
        /// Forces an immediate pose update, bypassing deadband thresholds.
        /// Use this for initialization or when an immediate update is required.
        /// </summary>
        /// <param name="position">The position to set</param>
        /// <param name="rotation">The rotation to set</param>
        public void ForceUpdatePose(Vector3 position, Quaternion rotation)
        {
            _cachedPosition = position;
            _cachedRotation = rotation;
            _isInitialized = true;
        }
    }
}
