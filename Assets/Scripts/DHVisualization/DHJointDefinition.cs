using System;
using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Represents a single joint in the DH parameter visualization system.
    /// Contains the joint's unique identifier and its position/rotation offsets
    /// relative to the parent AprilTag origin.
    /// </summary>
    [Serializable]
    public class DHJointDefinition
    {
        [Tooltip("Unique identifier for this joint (0, 1, 2, etc.)")]
        public int JointID;

        [Tooltip("Local position offset from the AprilTag origin in meters")]
        public Vector3 PositionOffset;

        [Tooltip("Local Euler rotation offset from the AprilTag orientation in degrees")]
        public Vector3 RotationOffset;
    }
}
