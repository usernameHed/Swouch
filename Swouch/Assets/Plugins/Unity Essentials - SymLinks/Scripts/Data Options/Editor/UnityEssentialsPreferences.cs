using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using unityEssentials.symbolicLinks.extensions.editor;

namespace unityEssentials.symbolicLinks.options
{
    public static class UnityEssentialsPreferences
    {
        public const string SHORT_NAME_PREFERENCE = "SymLinks Tool";

        public const string DEFAULT_COLOR                       = SHORT_NAME_PREFERENCE + " " + "DefaultColor";

        public const string SHOW_INDICATOR_ON_HIERARCHY_WINDOW  = SHORT_NAME_PREFERENCE + " " + "ShowIndicatorOnHierarchyWindow";
        public const string SHOW_INDICATOR_ON_PROJECT_WINDOW    = SHORT_NAME_PREFERENCE + " " + "ShowIndicatorOnProjectWindow";
        public const string SHOW_DOT_ON_PARENTS                = SHORT_NAME_PREFERENCE + " " + "ShowDotsOnParents";

        public const string NAME_PREFERENCE = "Project/" + SHORT_NAME_PREFERENCE;
        private static Color _currentColor = Color.red;

#if UNITY_2018_3_OR_NEWER
        private static HashSet<string> SEARCH_KEYWORDS = new HashSet<string>(new[]
        {
            "Symlinks",
            "Symlink",
            "Symbolic links",
            "Links",
            "Jonctions",
            "ShowIndicatorOnHierarchyWindow",
            "Show Indicator On Hierarchy Window",
            "ShowIndicatorOnProjectWindow",
            "Show Indicator On Project Window",
            "ShowDotsOnParent",
            "Show Dots On Parent",
            "DefaultColor", "Default Color"
        });
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

            }
        }

        /// <summary>
        /// display inside preferences
        /// </summary>
        /// <param name="serialized"></param>
        private static void PreferenceGUI()
        {
            EditorGUILayout.HelpBox(SHORT_NAME_PREFERENCE + " Preferences", MessageType.Info);

            if (!ExtGUILayout.Section("Indicator Preferences", "FolderFavorite Icon", true, "Symlink fold options", 5))
            {
                ManageColor(DEFAULT_COLOR, "Default Color");
                GUILayout.Label("");
                ManageBool(SHOW_INDICATOR_ON_HIERARCHY_WINDOW, "Show Indicator On Hierarchy");
                ManageBool(SHOW_INDICATOR_ON_PROJECT_WINDOW, "Show Indicator On Project");
                ManageBool(SHOW_DOT_ON_PARENTS, "Show Dot on parents");
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

        /// <summary>
        /// get the current color. If this is the first time, create it
        /// </summary>
        /// <returns></returns>
        public static Color GetDefaultColor()
        {
            if (!EditorPrefs.HasKey(DEFAULT_COLOR))
            {
                EditorPrefs.SetString(DEFAULT_COLOR, EditorJsonUtility.ToJson(_currentColor));
                return (_currentColor);
            }
            _currentColor = (Color)JsonUtility.FromJson(EditorPrefs.GetString(DEFAULT_COLOR), typeof(Color));
            return (_currentColor);
        }

        /// <summary>
        /// From a given key, add a colorField, display, and save when changes are made
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        private static void ManageColor(string key, string description)
        {
            if (!EditorPrefs.HasKey(DEFAULT_COLOR))
            {
                EditorPrefs.SetString(DEFAULT_COLOR, EditorJsonUtility.ToJson(_currentColor));
            }
            EditorGUI.BeginChangeCheck();
            Color color = EditorGUILayout.ColorField(new GUIContent(description), _currentColor);
            if (EditorGUI.EndChangeCheck())
            {
                _currentColor = color;
                EditorPrefs.SetString(key, EditorJsonUtility.ToJson(_currentColor));
            }
        }
    }
}