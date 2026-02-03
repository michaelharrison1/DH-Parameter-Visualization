using System;
using System.Collections.Generic;
using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Represents a mapping between an AprilTag marker and its associated joints.
    /// Each AprilTag can have multiple joints attached to it, each with their own
    /// position and rotation offsets.
    /// </summary>
    [Serializable]
    public class TagMapping
    {
        [Tooltip("The AprilTag 36h11 marker ID (0, 1, 2, etc.)")]
        public int AprilTagID;

        [Tooltip("Physical size of the printed tag in meters")]
        public float TagSizeMeters = 0.1f;

        [Tooltip("List of all joints attached to this AprilTag")]
        public List<DHJointDefinition> Joints = new List<DHJointDefinition>();
    }
}
