using UnityEditor;
using UnityEngine;
using unityEssentials.peek.options;

namespace unityEssentials.peek.extensions.editor
{
    static class ExtGUIStyles
    {
        public static readonly GUIStyle MiniText;
        public static readonly GUIStyle MicroButton;
        public static readonly GUIStyle MicroButtonLeftCenter;
        public static readonly GUIStyle MiniTextCentered;

        static ExtGUIStyles()
        {
            MiniText = new GUIStyle()
            {
                fontSize = 9,
            };
            MiniText.normal.textColor = Color.white;

            MicroButton = new GUIStyle(EditorStyles.miniButton);
            MicroButton.fontSize = 9;

            MicroButtonLeftCenter = new GUIStyle(EditorStyles.miniButton);
            MicroButtonLeftCenter.fontSize = EditorPrefs.GetInt(UnityEssentialsPreferences.FONT_SIZE_PEEK_WINDOW, 14);
            MicroButtonLeftCenter.alignment = TextAnchor.MiddleLeft;



            MiniTextCentered = new GUIStyle()
            {
                fontSize = 9,
            };
            MiniTextCentered.normal.textColor = Color.white;
            MiniTextCentered.alignment = TextAnchor.MiddleCenter;
        }
    }
}