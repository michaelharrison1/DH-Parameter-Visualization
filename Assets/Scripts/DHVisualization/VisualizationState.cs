using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// Enum defining all possible states in the DH parameter visualization flow.
    /// Users progress through states sequentially, starting with Joint 0.
    /// </summary>
    public enum VisualizationState
    {
        /// <summary>
        /// Initial state before visualization begins.
        /// </summary>
        Initializing,

        /// <summary>
        /// Currently assigning Joint 0 (base joint).
        /// </summary>
        AssigningJoint0,

        /// <summary>
        /// Currently assigning Joint 1.
        /// </summary>
        AssigningJoint1,

        /// <summary>
        /// Currently assigning Joint 2.
        /// </summary>
        AssigningJoint2,

        /// <summary>
        /// Currently assigning Joint 3.
        /// </summary>
        AssigningJoint3,

        /// <summary>
        /// Currently assigning Joint 4.
        /// </summary>
        AssigningJoint4,

        /// <summary>
        /// Currently assigning Joint 5.
        /// </summary>
        AssigningJoint5,

        /// <summary>
        /// Currently assigning Joint 6.
        /// </summary>
        AssigningJoint6,

        /// <summary>
        /// All joints have been assigned; visualization is complete.
        /// </summary>
        Complete
    }
}
