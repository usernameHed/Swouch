using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using unityEssentials.peek.extensions;
using unityEssentials.peek.extensions.editor;
using unityEssentials.peek.gui;
using static UnityEditor.EditorGUILayout;

namespace unityEssentials.peek
{
    /// <summary>
    /// this class is only used to display the content of the PeekWindow
    /// </summary>
    public class PeekEditorWindowDrawer
    {
        public const string PEEK_WINDOW = "Peek window";
        public const string OPEN_PREFERENCE_SETTINGS_TEXT = "Advanced Settings";
        public const string PEEK_ICON_WINDOW = "UnityLogo";
        private const float SIZE_SCROLL_BAR = 9f;

        private const string PIN_ICON = "FolderFavorite Icon";
        private const string PIN_TOOLTIP = "Save in bookmark";

        private const string UNPIN_ICON = "Favorite Icon";
        private const string UNPIN_ICON_HOVER = "d_Favorite";
        private const string UNPIN_TOOLTIP = "Remove from bookmark";

        private const string BOOKMARK_GAMEOBJECT_LOGO = "UnityLogo";
        private const string BOOKMARK_OBJECT_LOGO = "Project";
        private const string SELECTED_OBJECT_LOGO = "";

        private const string FOLD_STATE_BOOKMARK_GAMEOBJECT = "FOLD_STATE_BOOKMARK_GAMEOBJECT";
        private const string FOLD_STATE_BOOKMARK_OBJECT = "FOLD_STATE_BOOKMARK_OBJECT";
        private const string FOLD_STATE_SELECTED_OBJECT = "FOLD_STATE_SELECTED_OBJECT";

        private HiddedEditorWindow _hiddedEditorWindow;
        private PeekLogic _peekLogic;
        private Vector2 _scrollPosition = Vector2.zero;

        private GenericPeekListDrawer _pinnedGameObjectDrawer;
        private GenericPeekListDrawer _pinnedObjectDrawer;
        private GenericPeekListDrawer _selectedObjectDrawer;

        public void Init(PeekLogic peekLogic, HiddedEditorWindow hiddedEditorWindow)
        {
            _peekLogic = peekLogic;
            _hiddedEditorWindow = hiddedEditorWindow;

            _pinnedGameObjectDrawer = new GenericPeekListDrawer(
                _hiddedEditorWindow,
                BOOKMARK_GAMEOBJECT_LOGO,
                "BookMark in scenes",
                false,
                FOLD_STATE_BOOKMARK_GAMEOBJECT,
                true,
                _hiddedEditorWindow.PinnedObjectsInScenes,
                _hiddedEditorWindow.PinnedObjectsNameInScenes,
                _hiddedEditorWindow.PinnedObjectsScenesLink,
                UNPIN_ICON,
                UNPIN_ICON_HOVER,
                UNPIN_TOOLTIP,
                OnPinnedSceneGameObjectBookMarkClicked,
                OnPinnedSceneGameObjectClicked,
                OnPinnedSceneGameObjectInfo,
                OnPinnedSceneGameObjectInfoOpenScene,
                OnPinnedSceneGameObjectPinned,
                OnPinnedSceneGameObjectRemove,
                OnPinnedSceneGameObjectListClear);

            _pinnedObjectDrawer = new GenericPeekListDrawer(
                _hiddedEditorWindow,
                BOOKMARK_OBJECT_LOGO,
                "BookMark in project",
                false,
                FOLD_STATE_BOOKMARK_OBJECT,
                true,
                _hiddedEditorWindow.PinnedObjects,
                null,
                null,
                UNPIN_ICON,
                UNPIN_ICON_HOVER,
                UNPIN_TOOLTIP,
                OnPinnedObjectBookMarkClicked,
                OnPinnedObjectClicked,
                null,
                null,
                OnPinnedObjectPinned,
                OnPinnedObjectRemove,
                OnPinnedObjectListClear);

            _selectedObjectDrawer = new GenericPeekListDrawer(
                _hiddedEditorWindow,
                SELECTED_OBJECT_LOGO,
                "previously selected:",
                true,
                FOLD_STATE_SELECTED_OBJECT,
                true,
                _hiddedEditorWindow.SelectedObjectsWithoutDuplicate,
                null,
                null,
                PIN_ICON,
                UNPIN_ICON,
                PIN_TOOLTIP,
                OnSelectedObjectBookMarkClicked,
                OnSelectedObjectClicked,
                null,
                null,
                OnSelectedObjectPinned,
                OnSelectedObjectRemove,
                OnSelectedObjectListClear);
        }

        #region PinnedSceneGameObject functions
        private void OnPinnedSceneGameObjectBookMarkClicked(int index)
        {
            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Add(_hiddedEditorWindow.PinnedObjectsInScenes[index]);
            _hiddedEditorWindow.PinnedObjectsInScenes.RemoveAt(index);
            _hiddedEditorWindow.PinnedObjectsNameInScenes.RemoveAt(index);
            _hiddedEditorWindow.PinnedObjectsScenesLink.RemoveAt(index);
        }
        private void OnPinnedSceneGameObjectClicked(int index)
        {
            SelectItem(_hiddedEditorWindow.PinnedObjectsInScenes[index]);
        }
        private void OnPinnedSceneGameObjectInfo(int index)
        {
            SelectItem(_hiddedEditorWindow.PinnedObjectsScenesLink[index]);
        }
        private void OnPinnedSceneGameObjectInfoOpenScene(int index)
        {
            _peekLogic.SwapSceneAndSelectItem(_hiddedEditorWindow.PinnedObjectsScenesLink[index].GetPath(), _hiddedEditorWindow.PinnedObjectsNameInScenes[index], _hiddedEditorWindow.PinnedObjectsScenesLink[index].name);
        }

        private void OnPinnedSceneGameObjectPinned(int index)
        {
            SelectItem(_hiddedEditorWindow.PinnedObjectsInScenes[index], false);
        }
        private void OnPinnedSceneGameObjectRemove(int index)
        {
            if (_hiddedEditorWindow.LastSelectedObject == _hiddedEditorWindow.PinnedObjectsInScenes[index])
            {
                _hiddedEditorWindow.LastSelectedObject = null;
            }
            _hiddedEditorWindow.SelectedObjects.Remove(_hiddedEditorWindow.PinnedObjectsInScenes[index], true);
            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Remove(_hiddedEditorWindow.PinnedObjectsInScenes[index]);
            _hiddedEditorWindow.PinnedObjectsInScenes.RemoveAt(index);
            _hiddedEditorWindow.PinnedObjectsNameInScenes.RemoveAt(index);
            _hiddedEditorWindow.PinnedObjectsScenesLink.RemoveAt(index);
        }
        private void OnPinnedSceneGameObjectListClear()
        {
            if (_hiddedEditorWindow.PinnedObjectsInScenes.Contains(_hiddedEditorWindow.LastSelectedObject))
            {
                _hiddedEditorWindow.LastSelectedObject = null;
            }
            _hiddedEditorWindow.PinnedObjectsInScenes.Clear();
            _hiddedEditorWindow.PinnedObjectsNameInScenes.Clear();
            _hiddedEditorWindow.PinnedObjectsScenesLink.Clear();
        }
        #endregion

        #region PinnedObject functions
        private void OnPinnedObjectBookMarkClicked(int index)
        {
            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Add(_hiddedEditorWindow.PinnedObjects[index]);
            _hiddedEditorWindow.PinnedObjects.RemoveAt(index);
        }
        private void OnPinnedObjectClicked(int index)
        {
            SelectItem(_hiddedEditorWindow.PinnedObjects[index]);
        }
        private void OnPinnedObjectPinned(int index)
        {
            SelectItem(_hiddedEditorWindow.PinnedObjects[index], false);
        }
        private void OnPinnedObjectRemove(int index)
        {
            if (_hiddedEditorWindow.LastSelectedObject == _hiddedEditorWindow.PinnedObjects[index])
            {
                _hiddedEditorWindow.LastSelectedObject = null;
            }
            _hiddedEditorWindow.SelectedObjects.Remove(_hiddedEditorWindow.PinnedObjects[index], true);
            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Remove(_hiddedEditorWindow.PinnedObjects[index]);
            _hiddedEditorWindow.PinnedObjects.RemoveAt(index);
        }
        private void OnPinnedObjectListClear()
        {
            if (_hiddedEditorWindow.PinnedObjects.Contains(_hiddedEditorWindow.LastSelectedObject))
            {
                _hiddedEditorWindow.LastSelectedObject = null;
            }
            _hiddedEditorWindow.PinnedObjects.Clear();
        }
        #endregion

        #region SelectedObject functions
        private void OnSelectedObjectBookMarkClicked(int index)
        {
            SaveToBookMark(_hiddedEditorWindow.SelectedObjectsWithoutDuplicate[index], index);
        }
        private void SaveToBookMark(UnityEngine.Object toSelect, int index)
        {
            GameObject toSelectGameObject = toSelect as GameObject;
            if (toSelectGameObject != null && toSelectGameObject.scene.IsValid())
            {
                if (_hiddedEditorWindow.PinnedObjectsInScenes.AddIfNotContain(toSelect))
                {
                    _hiddedEditorWindow.PinnedObjectsNameInScenes.Add(toSelectGameObject.name);
                    _hiddedEditorWindow.PinnedObjectsScenesLink.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(toSelectGameObject.scene.path));
                }
            }
            else
            {
                _hiddedEditorWindow.PinnedObjects.AddIfNotContain(toSelect);
            }
            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.RemoveAt(index);
        }

        private void OnSelectedObjectClicked(int index)
        {
            SelectItem(_hiddedEditorWindow.SelectedObjectsWithoutDuplicate[index]);
        }
        private void OnSelectedObjectPinned(int index)
        {
            SelectItem(_hiddedEditorWindow.SelectedObjectsWithoutDuplicate[index], false);
        }
        private void OnSelectedObjectRemove(int index)
        {
            if (_hiddedEditorWindow.LastSelectedObject == _hiddedEditorWindow.SelectedObjectsWithoutDuplicate[index])
            {
                _hiddedEditorWindow.LastSelectedObject = null;
            }
            _hiddedEditorWindow.SelectedObjects.Remove(_hiddedEditorWindow.SelectedObjectsWithoutDuplicate[index], true);
            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.RemoveAt(index);
        }
        private void OnSelectedObjectListClear()
        {
            if (_hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Contains(_hiddedEditorWindow.LastSelectedObject))
            {
                _hiddedEditorWindow.LastSelectedObject = null;
            }
            _hiddedEditorWindow.SelectedObjectsWithoutDuplicate.Clear();
        }
        #endregion

        /// <summary>
        /// display content of peek window
        /// </summary>
        public void DrawPeekWindow()
        {
            float widthEditorWindow = _hiddedEditorWindow.GetPeekNScrollEditorWindow().position.width;
            float widthWithoutScrollBar = widthEditorWindow - SIZE_SCROLL_BAR;

            using (new VerticalScope())
            {
                _scrollPosition = GUILayout.BeginScrollView(
                    _scrollPosition,
                    false,
                    false,
                    GUIStyle.none,
                    GUI.skin.verticalScrollbar,
                    GUILayout.Width(widthEditorWindow));

                _pinnedGameObjectDrawer.Display();
                GUILayout.Label("");

                _pinnedObjectDrawer.Display();
                GUILayout.Label("");

                _selectedObjectDrawer.Display();

                GUILayout.Label("", GUILayout.Width(widthWithoutScrollBar));
                GUI.EndScrollView();
            }
        }

        private void SelectItem(UnityEngine.Object toSelect, bool select = true)
        {
            _peekLogic.ForceSelection(toSelect, select);
        }
    }
}