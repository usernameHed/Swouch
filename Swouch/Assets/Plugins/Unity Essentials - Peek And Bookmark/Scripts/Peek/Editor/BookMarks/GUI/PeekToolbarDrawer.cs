using UnityEditor;
using UnityEngine;
using unityEssentials.peek.extensions;
using unityEssentials.peek.extensions.editor;
using static UnityEditor.EditorGUILayout;

namespace unityEssentials.peek
{
    /// <summary>
    /// this class is only used to display the content of the PeekToolBar
    /// </summary>
    public class PeekToolbarDrawer
    {
        private const int _heightText = 8;
        public const string OPEN_EDITOR_WINDOW_ICON = "Audio Mixer";
        private const string PREVIOUS_ICON = "◀";
        private const string NEXT_ICON = "►";
        private HiddedEditorWindow _hiddedEditorWindow;
        private PeekLogic _peekLogic;

        public void Init(PeekLogic peekLogic, HiddedEditorWindow hiddedEditorWindow)
        {
            _peekLogic = peekLogic;
            _hiddedEditorWindow = hiddedEditorWindow;
        }

        /// <summary>
        /// display the content of the peek toolBar
        /// </summary>
        public void DisplayPeekToolBar()
        {
            if (_hiddedEditorWindow == null)
            {
                return;
            }

            if (_hiddedEditorWindow.CurrentIndex >= _hiddedEditorWindow.SelectedObjects.Count)
            {
                _hiddedEditorWindow.CurrentIndex = _hiddedEditorWindow.SelectedObjects.Count - 1;
            }

            using (VerticalScope vertical = new VerticalScope())
            {
                GUILayout.Label("Browse selections", ExtGUIStyles.MiniTextCentered, GUILayout.Height(_heightText));
                using (HorizontalScope horizontal = new HorizontalScope())
                {
                    if (GUILayout.Button(EditorGUIUtility.IconContent(OPEN_EDITOR_WINDOW_ICON), ExtGUIStyles.MicroButton, GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                         _hiddedEditorWindow = _peekLogic.ManageOpenPeekEditorWindow();
                    }
                    EditorGUI.BeginDisabledGroup(_hiddedEditorWindow.SelectedObjects.Count == 0);
                    {
                        float delta = 0;
                        bool isScrollingDown = ExtMouse.IsScrollingDown(Event.current, ref delta);
                        if (GUILayout.Button(PREVIOUS_ICON, ExtGUIStyles.MicroButton, GUILayout.Width(23), GUILayout.Height(EditorGUIUtility.singleLineHeight)) || isScrollingDown)
                        {
                            _peekLogic.AddToIndex(-1);
                            _peekLogic.ForceSelection(_hiddedEditorWindow.SelectedObjects[_hiddedEditorWindow.CurrentIndex]);
                        }
                        bool isScrollingUp = ExtMouse.IsScrollingUp(Event.current, ref delta);
                        if (GUILayout.Button(NEXT_ICON, ExtGUIStyles.MicroButton, GUILayout.Width(23), GUILayout.Height(EditorGUIUtility.singleLineHeight)) || isScrollingUp)
                        {
                            _peekLogic.AddToIndex(1);
                            _peekLogic.ForceSelection(_hiddedEditorWindow.SelectedObjects[_hiddedEditorWindow.CurrentIndex]);
                        }
                        if (_hiddedEditorWindow.SelectedObjects.Count == 0)
                        {
                            GUIContent gUIContent = new GUIContent("-/-", "there is no previously selected objects");
                            GUILayout.Label(gUIContent);
                        }
                        else
                        {
                            string showCount = (_hiddedEditorWindow.CurrentIndex + 1).ToString() + "/" + (_hiddedEditorWindow.SelectedObjects.Count);
                            GUIContent gUIContent = new GUIContent(showCount, "Scroll Up/Down to browse previous/next");
                            GUILayout.Label(gUIContent);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            GUILayout.FlexibleSpace();
        }
    }
}