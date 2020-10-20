using UnityEditor;
using UnityEngine;

namespace unityEssentials.peek.gui
{
    public class ButtonImageWithHoverOptions
    {
        public string IconKey { get; private set; }
        public string IconHover { get; private set; }
        public GUIStyle GuiStyle { get; private set; }
        public string ToolTip { get; private set; }
        public GUILayoutOption[] Options { get; private set; }

        public ButtonImageWithHoverOptions(string iconKey, string iconHover, GUIStyle guiStyle, string toolTip, params GUILayoutOption[] options)
        {
            IconKey = iconKey;
            IconHover = iconHover;
            GuiStyle = guiStyle;
            ToolTip = toolTip;
            Options = options;
        }

        public bool ButtonImageWithHover(float width, bool isEnabled, bool displayInfoIfDisabled, ref bool clicOnCustomButton, ref bool ctrlClicOnCustomButton)
        {
            GUIContent styleDefault = EditorGUIUtility.IconContent(IconKey);
            GUIContent styleHover = EditorGUIUtility.IconContent(IconHover);
            GUIContent styleGoToScene = EditorGUIUtility.IconContent("console.infoicon.sml");
            styleHover.tooltip = ToolTip;
            styleGoToScene.tooltip = "Select related scene if possible (CTRL + clic to directly change scene)";
            Rect rt = GUILayoutUtility.GetRect(styleDefault, GuiStyle, Options);
            bool clicked;
            ctrlClicOnCustomButton = false;

            if (!isEnabled)
            {
                if (displayInfoIfDisabled)
                {
                    bool clic = GUI.Button(new Rect(rt.x, rt.y, width, EditorGUIUtility.singleLineHeight), styleGoToScene, GuiStyle);
                    clicOnCustomButton = clic && Event.current.button == 0;
                    ctrlClicOnCustomButton = clic && Event.current.button == 0 && Event.current.control;
                }
                else
                {
                    clicOnCustomButton = false;
                    GUI.Label(new Rect(rt.x, rt.y, width, EditorGUIUtility.singleLineHeight), "");
                }
                return (false);
            }

            EditorGUI.BeginDisabledGroup(!isEnabled);
            {
                if (rt.Contains(Event.current.mousePosition))
                {
                    clicked = GUI.Button(new Rect(rt.x, rt.y, width, EditorGUIUtility.singleLineHeight), styleHover, GuiStyle) && Event.current.button == 0;
                }
                else
                {
                    clicked = GUI.Button(new Rect(rt.x, rt.y, width, EditorGUIUtility.singleLineHeight), styleDefault, GuiStyle) && Event.current.button == 0;
                }
            }
            EditorGUI.EndDisabledGroup();
            return (clicked);
        }
    }
}