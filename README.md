# DH Parameter Visualzation

## Overview

## Compatible with
- Unity Editor 2022.3+
- Magic Leap Unity SDK 2.6.0-pre.R15


DHVisualizationManager.cs  initializes all systems, processes AprilTag tracking data each frame
 RobotConfiguration.cs  - ScriptableObject holding all tag-to-joint mappings for a robot
 TagMapping.cs  - Maps one AprilTag ID to its list of joints and tag size
 DHJointDefinition.cs  - Defines a single joint's ID, position offset, and rotation offset
 TagAnchorManager.cs  - Creates/manages persistent anchor GameObjects for each AprilTag
 TagAnchor.cs - Single tracked tag's world position - persists when tracking lost
 PoseStabilizer.cs  Deadband filter - rejects small pose changes to reduce jitter
 JointVisualizerManager.cs  - Spawns XYZ axis prefabs for all joints, controls visibility
 JointVisual.cs  - Controls one joint's XYZ axis display, waits for tracking before showing
 VisualizationState.cs  - Enum of states: Initializing → AssigningJoint0...N → Complete
 VisualizationStateController.cs  - State machine - tracks current joint, fires events on advance/back
 InputHandler.cs  ML2 controller input - Trigger=next joint, Bumper=previous
 VisualizationUIController.cs  - Updates UI text ("Step N", "Joint N Assignment") on state changes
