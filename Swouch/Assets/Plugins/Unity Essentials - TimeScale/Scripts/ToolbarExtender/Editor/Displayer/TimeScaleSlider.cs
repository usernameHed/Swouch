using unityEssentials.timeScale.time;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using unityEssentials.timeScale.extensions.editor;
using unityEssentials.timeScale.options;
using UnityEngine.UI;
using unityEssentials.timeScale.extensions;

namespace unityEssentials.timeScale.toolbarExtent
{
    public class TimeScaleSlider
    {
        private const int HEIGHT_TEXT = 8;
        private const int HEIGHT_BUTTON = 15;
        private const int SIZE_SLIDER = 120;
        private const string TIME_SCALE_ICON = "d_SpeedScale";
        public static readonly GUIStyle _miniText;

        static TimeScaleSlider()
        {
            _miniText = new GUIStyle()
            {
                fontSize = 9,
            };
            _miniText.normal.textColor = Color.white;
        }

        public void Init()
        {
            if (EditorPrefs.GetBool(UnityEssentialsPreferences.SHOW_SLIDER, true))
            {
                TimeEditor.timeScale = 1;
            }
        }

        public void DisplaySliderLeft()
        {
            if (!EditorPrefs.GetBool(UnityEssentialsPreferences.SHOW_SLIDER, true))
            {
                return;
            }
            float percent = EditorPrefs.GetFloat(UnityEssentialsPreferences.POSITION_IN_TOOLBAR, 0.95f);
            if (percent > 0.5f)
            {
                return;
            }
            Rect left = ToolbarExtender.GetLeftRect();
            percent = ExtMathf.Remap(percent, 0f, 0.5f, 0f, 1f);
            float width = (left.width - SIZE_SLIDER) / 1f * percent;
            GUILayout.Label("", GUILayout.MinWidth(0), GUILayout.Width(width));
            DisplaySlider(width);
        }

        public void DisplaySliderRight()
        {
            if (!EditorPrefs.GetBool(UnityEssentialsPreferences.SHOW_SLIDER, true))
            {
                return;
            }
            float percent = EditorPrefs.GetFloat(UnityEssentialsPreferences.POSITION_IN_TOOLBAR, 0.95f);
            if (percent <= 0.5f)
            {
                return;
            }
            Rect left = ToolbarExtender.GetRightRect();
            percent = ExtMathf.Remap(percent, 0.5f, 1f, 0f, 1f);
            float width = (left.width - SIZE_SLIDER) / 1f * percent;
            GUILayout.Label("", GUILayout.MinWidth(0), GUILayout.Width(width));
            DisplaySlider(width);
        }

        private void DisplaySlider(float width)
        {
            GUI.Label(new Rect(width + 50, -3, 50, 18), EditorGUIUtility.IconContent(TIME_SCALE_ICON));
            using (VerticalScope vertical = new VerticalScope())
            {
                GUILayout.Label("TimeScale", _miniText, GUILayout.Height(HEIGHT_TEXT));
                using (HorizontalScope horiz = new HorizontalScope())
                {
                    float timeScale = EditorGUILayout.Slider("", TimeEditor.timeScale, 0, 1, GUILayout.Width(SIZE_SLIDER), GUILayout.Height(HEIGHT_BUTTON));
                    if (timeScale != TimeEditor.timeScale)
                    {
                        TimeEditor.timeScale = timeScale;
                    }
                }
            }
        }
    }
}