using System.Collections.Generic;
using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// ScriptableObject that holds the entire robot configuration for DH parameter visualization.
    /// Contains all tag-to-joint mappings and provides helper methods for accessing joint data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRobotConfiguration", menuName = "DH Visualization/Robot Configuration", order = 1)]
    public class RobotConfiguration : ScriptableObject
    {
        [Tooltip("Descriptive name for this robot configuration")]
        public string ConfigurationName;

        [Tooltip("List of all AprilTag-to-joint mappings for this robot")]
        public List<TagMapping> TagMappings = new List<TagMapping>();

        /// <summary>
        /// Returns the total number of joints across all tag mappings.
        /// </summary>
        /// <returns>Total joint count</returns>
        public int GetTotalJointCount()
        {
            int count = 0;
            foreach (var tagMapping in TagMappings)
            {
                if (tagMapping.Joints != null)
                {
                    count += tagMapping.Joints.Count;
                }
            }
            return count;
        }
    }
}
