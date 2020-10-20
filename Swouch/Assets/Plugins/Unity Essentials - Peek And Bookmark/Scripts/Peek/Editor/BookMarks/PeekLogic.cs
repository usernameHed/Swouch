using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using unityEssentials.peek.extensions;
using unityEssentials.peek.extensions.editor;
using unityEssentials.peek.options;

namespace unityEssentials.peek
{
    public class PeekLogic
    {
        private HiddedEditorWindow _hiddedEditorWindow;
        public HiddedEditorWindow GetHiddedEditorWindow() { return (_hiddedEditorWindow); }

        private PeekEditorWindowDrawer _peekWindowDrawer = new PeekEditorWindowDrawer();
        private PeekToolbarDrawer _peekToolBarDrawer = new PeekToolbarDrawer();
        public PeekToolbarDrawer PeekToolBar { get { return (_peekToolBarDrawer); } }
        private PeekReloadSceneBookMark _peekReloadSceneBookMark = new PeekReloadSceneBookMark();
        public PeekReloadSceneBookMark PeekReloadBookMark { get { return (_peekReloadSceneBookMark); } }
        private bool _isInit = false;
        private bool _isClosed = false;
        private EditorChrono _frequencyCoolDown = new EditorChrono();
        private static List<Scene> _sceneLoaded = new List<Scene>(10);

        private bool _needToChangeScene = false;
        private string _toSelectName;
        private string _sceneOfGameObjectToSelectName;
        private string _scenePathOfGameObjectToSelect;

        public PeekLogic()
        {
            _isInit = false;
            EditorApplication.update += UpdateEditor;
            EditorApplication.playModeStateChanged += ChangeSceneAfterPlayMode;
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += KeepLinkAndNamesUpToDate;
#else
            EditorApplication.hierarchyWindowChanged += KeepLinkAndNamesUpToDate;
#endif
        }

        ~PeekLogic()
        {
            EditorApplication.update -= UpdateEditor;
            EditorApplication.playModeStateChanged -= ChangeSceneAfterPlayMode;

#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged -= KeepLinkAndNamesUpToDate;
#else
            EditorApplication.hierarchyWindowChanged -= KeepLinkAndNamesUpToDate;
#endif
        }

        public void Init()
        {
            _isInit = true;
            _hiddedEditorWindow = HiddedEditorWindow.ConstructHiddedWindow();
            _isClosed = _hiddedEditorWindow.IsClosed;
            _peekReloadSceneBookMark.Init(this, _hiddedEditorWindow);
            if (!_hiddedEditorWindow.WasOpenLastTimeWeCloseUnity())
            {
                _hiddedEditorWindow.UpdatePeekEditorWindowIfOpen();
            }
            _peekWindowDrawer.Init(this, _hiddedEditorWindow);
            _peekToolBarDrawer.Init(this, _hiddedEditorWindow);
            UpdateSceneGameObjectOnLoad();
        }

        public HiddedEditorWindow ManageOpenPeekEditorWindow()
        {
            if (_hiddedEditorWindow == null)
            {
                _hiddedEditorWindow = HiddedEditorWindow.ConstructHiddedWindow();
            }

            _hiddedEditorWindow.ToggleOpenCloseDisplay();
            _isClosed = _hiddedEditorWindow.IsClosed;

            if (!_isClosed)
            {
                UpdateOnEditorWindowOpen();
            }

            return (_hiddedEditorWindow);
        }

        public void ReactiveLogicIfLinkIsMissing()
        {
            _hiddedEditorWindow?.RelinkPeekWindow();
        }

        public void KeepLinkAndNamesUpToDate()
        {
            _hiddedEditorWindow?.KeepLinkAndNamesUpToDate();
        }

        public void DeleteBookMarkedGameObjectLinkedToScene()
        {
            _hiddedEditorWindow?.DeleteBookMarkedGameObjectOfLostScene();
        }

        private void UpdateEditor()
        {
            try
            {
                WaitUntilSceneIsValid();

                if (_hiddedEditorWindow == null)
                {
                    if (_frequencyCoolDown.IsNotRunning())
                    {
                        _hiddedEditorWindow = HiddedEditorWindow.ConstructHiddedWindow();
                        _frequencyCoolDown.StartChrono(2f);
                    }
                    return;
                }

                UnityEngine.Object currentSelectedObject = Selection.activeObject;
                if (currentSelectedObject != null && currentSelectedObject != _hiddedEditorWindow.LastSelectedObject)
                {
                    AttemptToRemoveNull();
                    AddNewSelection(currentSelectedObject);
                }

                if (!_isInit)
                {
                    Init();
                }
                if (_hiddedEditorWindow == null)
                {
                    return;
                }
                if (!_hiddedEditorWindow.IsClosed)
                {
                    _hiddedEditorWindow.Display(_peekWindowDrawer.DrawPeekWindow);
                }
                if (_isClosed != _hiddedEditorWindow.IsClosed)
                {
                    _hiddedEditorWindow.SaveCloseStatus();
                    _isClosed = _hiddedEditorWindow.IsClosed;
                }
            }
            catch { }
        }

        private void AttemptToRemoveNull()
        {
            if (_hiddedEditorWindow.SelectedObjects != null && IsThereNullInList(_hiddedEditorWindow.SelectedObjects))
            {
                bool hasChanged = false;
                _hiddedEditorWindow.SelectedObjects = CleanNullFromList(_hiddedEditorWindow.SelectedObjects, ref hasChanged);
                _hiddedEditorWindow.SelectedObjectsWithoutDuplicate = CleanNullFromList(_hiddedEditorWindow.SelectedObjectsWithoutDuplicate, ref hasChanged);
            }
        }

        /// <summary>
        /// Clean  null item (do not remove items, remove only the list)
        /// </summary>
        /// <param name="listToClean"></param>
        /// <returns>true if list changed</returns>
        public static List<UnityEngine.Object> CleanNullFromList(List<UnityEngine.Object> listToClean, ref bool hasChanged)
        {
            hasChanged = false;
            if (listToClean == null)
            {
                return (listToClean);
            }
            for (int i = listToClean.Count - 1; i >= 0; i--)
            {
                if (ExtObjectEditor.IsTruelyNull(listToClean[i]))
                {
                    listToClean.RemoveAt(i);
                    hasChanged = true;
                }
            }
            return (listToClean);
        }

        public static bool IsThereNullInList(List<UnityEngine.Object> listToClean)
        {
            for (int i = 0; i < listToClean.Count; i++)
            {
                if (ExtObjectEditor.IsTruelyNull(listToClean[i]))
                {
                    return (true);
                }
            }
            return (false);
        }

        private void AddNewSelection(UnityEngine.Object currentSelectedObject)
        {
            _hiddedEditorWindow.LastSelectedObject = currentSelectedObject;
            _hiddedEditorWindow.SelectedObjects.Add(_hiddedEditorWindow.LastSelectedObject);

            _hiddedEditorWindow.CurrentIndex = _hiddedEditorWindow.SelectedObjects.Count - 1;

            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Remove(currentSelectedObject);
            if (!_hiddedEditorWindow.PinnedObjects.Contains(currentSelectedObject)
                && !_hiddedEditorWindow.PinnedObjectsInScenes.Contains(currentSelectedObject))
            {
                _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Add(currentSelectedObject);
            }
            ShrunkListIfNeeded();
        }

        public void ShrunkListIfNeeded()
        {
            if (_hiddedEditorWindow.SelectedObjects.Count >= UnityEssentialsPreferences.GetMaxSelectedObjectStored())
            {
                _hiddedEditorWindow.SelectedObjects.RemoveAt(0);
            }
            while (_hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Count >= UnityEssentialsPreferences.GetMaxObjectSHown())
            {
                _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.RemoveAt(0);
            }
            while (_hiddedEditorWindow.PinnedObjects.Count >= UnityEssentialsPreferences.GetMaxPinnedObject())
            {
                _hiddedEditorWindow.PinnedObjects.RemoveAt(0);
            }
            while (_hiddedEditorWindow.PinnedObjectsInScenes.Count >= UnityEssentialsPreferences.GetMaxPinnedObject())
            {
                _hiddedEditorWindow.PinnedObjectsInScenes.RemoveAt(0);
                _hiddedEditorWindow.PinnedObjectsNameInScenes.RemoveAt(0);
                _hiddedEditorWindow.PinnedObjectsScenesLink.RemoveAt(0);
            }
        }

        public void ForceSelection(UnityEngine.Object forcedSelection, bool select = true)
        {
            _hiddedEditorWindow.LastSelectedObject = forcedSelection;
            if (select)
            {
                Selection.activeObject = _hiddedEditorWindow.LastSelectedObject;
                EditorGUIUtility.PingObject(_hiddedEditorWindow.LastSelectedObject);
            }
            else
            {
                EditorGUIUtility.PingObject(_hiddedEditorWindow.LastSelectedObject);
            }
        }

        public void AddToIndex(int add)
        {
            _hiddedEditorWindow.CurrentIndex += add;
            _hiddedEditorWindow.CurrentIndex = Mathf.Clamp(_hiddedEditorWindow.CurrentIndex, 0, _hiddedEditorWindow.SelectedObjects.Count - 1);
        }

        public void UpdateOnEditorWindowOpen()
        {
            UpdateSceneGameObjectOnLoad();
        }

        public void UpdateSceneGameOnPlay()
        {

        }

        public void UpdateSceneGameObjectOnLoad()
        {
            int countScene = SceneManager.sceneCount;
            for (int i = 0; i < countScene; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                _sceneLoaded.AddIfNotContain(scene);
            }
            _hiddedEditorWindow.DeleteBookMarkedGameObjectOfLostScene();
        }

        public void UpdateSceneGameObjectFromSceneLoad(Scene scene)
        {
            _sceneLoaded.AddIfNotContain(scene);
        }

        private void WaitUntilSceneIsValid()
        {
            bool mustReload = false;
            for (int i = _sceneLoaded.Count - 1; i >= 0; i--)
            {
                if (!_sceneLoaded[i].IsValid())
                {
                    mustReload = true;
                    _sceneLoaded.RemoveAt(i);
                    continue;
                }

                _peekReloadSceneBookMark.ReloadSceneFromOneLoadedScene(_sceneLoaded[i]);
                _sceneLoaded.RemoveAt(i);
            }

            if (mustReload)
            {
                UpdateSceneGameObjectOnLoad();
            }
        }

        public void SwapSceneAndSelectItem(string scenePath, string toSelectName, string sceneName)
        {
            _scenePathOfGameObjectToSelect = scenePath;
            _toSelectName = toSelectName;
            _sceneOfGameObjectToSelectName = sceneName;
            _needToChangeScene = false;

            if (Application.isPlaying)
            {
                if (!ExtGUILayout.DrawDisplayDialog(
                    "Change scene",
                    "Quit play mode and swich to scene " + sceneName))
                {
                    return;
                }
                UnityEditor.EditorApplication.isPlaying = false;
                _needToChangeScene = true;
                return;
            }
            else if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            LoadSceneFromDatas(_scenePathOfGameObjectToSelect, _toSelectName, _sceneOfGameObjectToSelectName);
        }

        private void LoadSceneFromDatas(string scenePath, string toSelectName, string sceneName)
        {
            EditorUtility.DisplayProgressBar("Loading scene", "", 0f);
            _peekReloadSceneBookMark.AutomaticlySelectWhenFound(toSelectName, sceneName);
            EditorSceneManager.OpenScene(scenePath);
            EditorUtility.ClearProgressBar();
        }

        public void ChangeSceneAfterPlayMode(PlayModeStateChange state)
        {
            if (!_needToChangeScene)
            {
                return;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                LoadSceneFromDatas(_scenePathOfGameObjectToSelect, _toSelectName, _sceneOfGameObjectToSelectName);
                _needToChangeScene = false;
            }
        }
    }
}