using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using unityEssentials.timeScale.extensions.editor;
using unityEssentials.timeScale.toolbarExtent;
using static UnityEditor.EditorGUILayout;

namespace unityEssentials.timeScale.options
{
    public static class UnityEssentialsPreferences
    {
        public const string SHORT_NAME_PREFERENCE = "TimeScale slider";

        public const string SHOW_SLIDER             = SHORT_NAME_PREFERENCE + " " + "ShowSlider";
        public const string POSITION_IN_TOOLBAR     = SHORT_NAME_PREFERENCE + " " + "PositionInToolBar";

        public const string NAME_PREFERENCE = "Project/" + SHORT_NAME_PREFERENCE;

#if UNITY_2018_3_OR_NEWER
        private static HashSet<string> SEARCH_KEYWORDS = new HashSet<string>(new[] { "TimeScale", "Time Scale", "Show Slider" });
        /// <summary>
        /// Settings Provider logic
        /// </summary>
        /// <returns></returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(NAME_PREFERENCE, SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    OnProviderCustomGUI();
                },
                keywords = UnityEssentialsPreferences.SEARCH_KEYWORDS
            };
        }
#else
        [PreferenceItem(SHORT_NAME_PREFERENCE)]
        public static void CreateSettingsPreferenceItem()
        {
            OnProviderCustomGUI();
        }
#endif

        private static void OnProviderCustomGUI()
        {
            EditorGUI.BeginChangeCheck();
            PreferenceGUI();
            if (EditorGUI.EndChangeCheck())
            {
                ToolbarExtender.Repaint();
            }
        }

        /// <summary>
        /// display inside preferences
        /// </summary>
        /// <param name="serialized"></param>
        private static void PreferenceGUI()
        {
            EditorGUILayout.HelpBox(SHORT_NAME_PREFERENCE + " Preferences", MessageType.Info);

            if (!ExtGUILayout.Section("TimeScale Toolbar", "Slider Icon", true, "Peek toolbar fold options", 5))
            {
                ManageBool(SHOW_SLIDER, "Show Slider");
                ManageSlider(POSITION_IN_TOOLBAR, "Position in toolbar: ", 0.95f, 0f, 1f);
                GUILayout.Space(25);
            }
        }

        private static void ManageBool(string key, string description)
        {
            EditorGUI.BeginChangeCheck();
            bool show = GUILayout.Toggle(EditorPrefs.GetBool(key, true), new GUIContent(description));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(key, show);
            }
        }

        private static void ManageSlider(string key, string description, float defaultValue, float from, float to)
        {
            using (HorizontalScope horiz = new HorizontalScope())
            {
                EditorGUILayout.LabelField(description, GUILayout.ExpandWidth(false));

                EditorGUI.BeginChangeCheck();
                float sliderPosition = EditorGUILayout.Slider(EditorPrefs.GetFloat(key, defaultValue), from, to, GUILayout.Width(130));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetFloat(key, sliderPosition);
                }
            }
        }
    }
}