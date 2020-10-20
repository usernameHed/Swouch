using UnityEditor;
using UnityEngine;
using unityEssentials.peek.options;
using unityEssentials.peek.extensions;

namespace unityEssentials.peek.toolbarExtent
{
    public class PeekTool
    {
        private const int HEIGHT_TEXT = 8;
        private const int HEIGHT_BUTTON = 15;
        private const int SIZE_SLIDER = 120;

        public static readonly GUIStyle _miniText;

        private PeekLogic _peekLogic = new PeekLogic();
        public PeekLogic PeekLogic { get { return (_peekLogic);} }

        static PeekTool()
        {
            _miniText = new GUIStyle()
            {
                fontSize = 9,
            };
            _miniText.normal.textColor = Color.white;
        }

        public void Init()
        {

        }

        public void DisplayLeft()
        {
            if (!EditorPrefs.GetBool(UnityEssentialsPreferences.SHOW_PEEK_MENU, true))
            {
                return;
            }
            float percent = EditorPrefs.GetFloat(UnityEssentialsPreferences.POSITION_IN_TOOLBAR, 0.05f);
            if (percent > 0.5f)
            {
                return;
            }
            Rect left = ToolbarExtender.GetLeftRect();
            percent = ExtMathf.Remap(percent, 0f, 0.5f, 0f, 1f);
            GUILayout.Label("", GUILayout.MinWidth(0), GUILayout.Width((left.width - SIZE_SLIDER) / 1 * percent));
            DisplayPeek();
        }

        public void DisplayRight()
        {
            if (!EditorPrefs.GetBool(UnityEssentialsPreferences.SHOW_PEEK_MENU, true))
            {
                return;
            }
            float percent = EditorPrefs.GetFloat(UnityEssentialsPreferences.POSITION_IN_TOOLBAR, 0.05f);
            if (percent <= 0.5f)
            {
                return;
            }
            Rect left = ToolbarExtender.GetRightRect();
            percent = ExtMathf.Remap(percent, 0.5f, 1f, 0f, 1f);
            GUILayout.Label("", GUILayout.MinWidth(0), GUILayout.Width((left.width - SIZE_SLIDER) / 1 * percent));
            DisplayPeek();
        }

        private void DisplayPeek()
        {
            _peekLogic.PeekToolBar.DisplayPeekToolBar();
        }
    }
}