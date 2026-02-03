// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2019-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace MagicLeap.Examples
{
    [ExecuteInEditMode]
    public class UserInterface : MonoBehaviour
    {
        private const float SIDE_MENU_DEFAULT_WIDTH = 175;
        private const float SIDE_MENU_MAX_WIDTH = 475;

        [Header("Settings")]
        [SerializeField, Tooltip("The primary workspace, this area will be collapsed in the minimized view.")]
        private GameObject _workspace = null;

        [Header("Interface")]
        [SerializeField, Tooltip("The transform of the side menu.")]
        private RectTransform _sideMenu = null;

        [SerializeField]
        private CanvasGroup dragInstructions;

        [SerializeField]
        private bool enableDragInstructions = false;

        [Header("Button & Text Fields")]

        [SerializeField, Tooltip("The title text element.")]
        private Text _title = null;

        [SerializeField, Tooltip("The UIButton for the scene tab.")]
        private UIButton _SceneTab = null;
        
        [SerializeField, Tooltip("The UIButton for the overview/visualization tab (opens by default).")]
        private UIButton _OverviewTab = null;

        [Header("Scene selection")]

        [SerializeField, Tooltip("Prefab of Scene Selection Button")]
        private UISceneSelectionButton sceneSelectionButtonPrefab;

        [SerializeField, Tooltip("Parent transform for all created Scene Selection Buttons")]
        private Transform scenesListTransform = null;

        [SerializeField, Tooltip("Button that opens selected scene")]
        private UIButton openSceneButton;

        [SerializeField, Tooltip("Scroll rect for scene selection buttons")]
        private ScrollRect listScrollRect;

        [Header("Example Information")]

        [SerializeField]
        private string title;

        private string selectedScene;
        private Canvas _canvas;

        private void Awake()
        {  
            // Open the overview/visualization tab by default (instead of scenes)
            if (_OverviewTab != null)
            {
                _OverviewTab.Pressed();
            }
            else if (_SceneTab != null)
            {
                // Fallback to scene tab if overview not assigned
                _SceneTab.Pressed();
            }

            if (dragInstructions == null)
            {
                enableDragInstructions = false;
            }

            if (!enableDragInstructions && dragInstructions != null)
            {
                dragInstructions.alpha = 0f;
            }
        }

        private void Start()
        {
            CreateSceneButtons();
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                _title.text = GetTitle();
        }

        private void OnDisable()
        {
        }

        private void OnValidate()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }

            if (_canvas != null && _canvas.worldCamera == null)
            {
                _canvas.worldCamera = Camera.main;
            }
            
            if (!string.IsNullOrWhiteSpace(title) && _title != null)
            {
                _title.text = title;
            }
            

        }



        public void DismissDragInstruction()
        {
            if (dragInstructions != null)
            {
                dragInstructions.alpha = 0f;
            }
        }

        /// <summary>
        /// Toggle the visibility of the workspace.
        /// </summary>
        public void ToggleCanvas()
        {
            ShowCanvas(!_workspace.activeInHierarchy);
        }

        /// <summary>
        /// Opens selected scene. (Invoked by UIButton)
        /// </summary>
        public void OpenScene()
        {
            LoaderUtility.Initialize();
            SceneManager.LoadScene(selectedScene);
        }

        /// <summary>
        /// Set the visibility of the workspace.
        /// </summary>
        /// <param name="visible">The desired visible state of the workspace.</param>
        public void ShowCanvas(bool visible)
        {
            _workspace.SetActive(visible);

            // Adjust the width of the side menu, this allows it to shift left/right.
            _sideMenu.sizeDelta = new Vector2((_workspace.activeInHierarchy) ? SIDE_MENU_DEFAULT_WIDTH : SIDE_MENU_MAX_WIDTH, _sideMenu.sizeDelta.y);
        }

        public void QuitApplication() => Application.Quit();

        private void CreateSceneButtons()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (i == currentSceneIndex)
                    continue;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                UISceneSelectionButton button = Instantiate(sceneSelectionButtonPrefab, scenesListTransform);
                button.Initialize(sceneName, listScrollRect, OnSceneSelected);
                button.gameObject.name = "Scene Button - " + sceneName;
            }
        }

        private void OnSceneSelected(string sceneName)
        {
            selectedScene = sceneName;
            openSceneButton.gameObject.SetActive(true);
        }

        private string GetTitle()
        {
            var words =
                    Regex.Matches(SceneManager.GetActiveScene().name, @"([A-Z][a-z]+)")
                    .Cast<Match>()
                    .Select(m => m.Value);

            return string.Join(" ", words);
        }
    }
}
