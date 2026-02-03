using UnityEngine;

namespace DHVisualization
{
    /// <summary>
    /// MonoBehaviour attached to each spawned joint visualization prefab.
    /// Controls visibility of individual axis children and the entire joint visual.
    /// Only shows if parent TagAnchor has received tracking.
    /// </summary>
    public class JointVisual : MonoBehaviour
    {
        [Tooltip("The unique identifier of the joint this visual represents")]
        public int JointID;

        // References to the axis child transforms
        private Transform _xAxis;
        private Transform _yAxis;
        private Transform _zAxis;
        
        // Reference to parent TagAnchor (cached for performance)
        private TagAnchor _parentTagAnchor;
        
        // Track if we should be visible (set by ShowJointsUpTo)
        private bool _shouldBeVisible = false;
        
        /// <summary>
        /// Whether this JointVisual should be visible (based on current step).
        /// </summary>
        public bool ShouldBeVisible => _shouldBeVisible;
        
        // Track if parent reference has been cached
        private bool _parentCached = false;

        private void Awake()
        {
            // Find the axis children by name
            _xAxis = transform.Find("X_Axis");
            _yAxis = transform.Find("Y_Axis");
            _zAxis = transform.Find("Z_Axis");

            // Log warnings if any axis is not found
            if (_xAxis == null)
            {
                Debug.LogWarning($"JointVisual (Joint {JointID}): X_Axis child not found.");
            }
            if (_yAxis == null)
            {
                Debug.LogWarning($"JointVisual (Joint {JointID}): Y_Axis child not found.");
            }
            if (_zAxis == null)
            {
                Debug.LogWarning($"JointVisual (Joint {JointID}): Z_Axis child not found.");
            }
            
            // Don't cache parent here - we're not parented yet during Instantiate!
        }
        
        private void Start()
        {
            // Cache parent TagAnchor reference AFTER parenting is complete
            CacheParentReference();
        }
        
        /// <summary>
        /// Caches the parent TagAnchor reference. Called in Start and can be called manually.
        /// </summary>
        public void CacheParentReference()
        {
            _parentTagAnchor = GetComponentInParent<TagAnchor>();
            _parentCached = true;
        }
        
        private void Update()
        {
            // If parent not cached yet, try to cache it
            if (!_parentCached)
            {
                CacheParentReference();
            }
            
            // If we should be visible but aren't yet, check if parent has tracking
            if (_shouldBeVisible && !gameObject.activeSelf)
            {
                if (_parentTagAnchor != null && _parentTagAnchor.HasReceivedTracking)
                {
                    gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Sets the visibility of each individual axis.
        /// </summary>
        /// <param name="showX">Whether to show the X axis</param>
        /// <param name="showY">Whether to show the Y axis</param>
        /// <param name="showZ">Whether to show the Z axis</param>
        public void SetAxisVisibility(bool showX, bool showY, bool showZ)
        {
            if (_xAxis != null)
            {
                _xAxis.gameObject.SetActive(showX);
            }
            if (_yAxis != null)
            {
                _yAxis.gameObject.SetActive(showY);
            }
            if (_zAxis != null)
            {
                _zAxis.gameObject.SetActive(showZ);
            }
        }

        /// <summary>
        /// Shows the entire joint visual GameObject.
        /// Will only actually show if parent TagAnchor has received tracking.
        /// </summary>
        public void Show()
        {
            _shouldBeVisible = true;
            
            // Make sure parent is cached
            if (!_parentCached)
            {
                CacheParentReference();
            }
            
            // Only actually show if parent exists AND has received tracking
            // If parent is null or hasn't received tracking yet, Update() will show when ready
            if (_parentTagAnchor != null && _parentTagAnchor.HasReceivedTracking)
            {
                gameObject.SetActive(true);
            }
            // Otherwise, Update() will show it when tracking is received
        }

        /// <summary>
        /// Hides the entire joint visual GameObject.
        /// </summary>
        public void Hide()
        {
            _shouldBeVisible = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Refreshes visibility based on the current _shouldBeVisible state.
        /// Called by TagAnchor when tracking is first received to properly
        /// show/hide based on the current visualization step.
        /// </summary>
        public void RefreshVisibility()
        {
            if (_shouldBeVisible)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
