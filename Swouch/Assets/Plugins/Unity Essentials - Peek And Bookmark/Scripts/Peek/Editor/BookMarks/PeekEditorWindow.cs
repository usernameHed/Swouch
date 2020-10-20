using System;
using UnityEditor;
using UnityEngine;
using unityEssentials.peek.extensions.editor.editorWindow;
using unityEssentials.peek.options;
using unityEssentials.peek.toolbarExtent;

namespace unityEssentials.peek
{
    public class PeekEditorWindow : DecoratorEditorWindow
#if UNITY_2018_3_OR_NEWER
        , IHasCustomMenu
#endif
    {
        public const int MIN_WIDTH = 270;
        private const int MIN_HEIGHT = 200;
        public Action ShowInside;

        public void Init()
        {
            GUIContent title = new GUIContent(EditorGUIUtility.IconContent(PeekToolbarDrawer.OPEN_EDITOR_WINDOW_ICON));
            title.text = PeekEditorWindowDrawer.PEEK_WINDOW;
            this.titleContent = title;
            SetMinSize(new Vector2(MIN_WIDTH, MIN_HEIGHT));
        }

        /// <summary>
        /// called every frame, 
        /// </summary>
        /// <param name="action"></param>
        public void ShowEditorWindow(Action action)
        {
            ShowInside = action;
        }

        private void OnGUI()
        {
            ShowInside?.Invoke();
            if (ShowInside == null)
            {
                ToolsButton.PeekTool?.PeekLogic?.ReactiveLogicIfLinkIsMissing();
            }
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

#if UNITY_2018_3_OR_NEWER
        /// <summary>
        /// This interface implementation is automatically called by Unity.
        /// </summary>
        /// <param name="menu"></param>
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            GUIContent content = new GUIContent(PeekEditorWindowDrawer.OPEN_PREFERENCE_SETTINGS_TEXT);
            menu.AddItem(content, false, OpenPreferenceSettings);
        }

        private void OpenPreferenceSettings()
        {
            SettingsService.OpenProjectSettings("Project/" + UnityEssentialsPreferences.SHORT_NAME_PREFERENCE);
        }
#endif
    }
}