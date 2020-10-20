using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using unityEssentials.peek.extensions.editor;
using unityEssentials.peek.shaderOutline;
using unityEssentials.peek.toolbarExtent;
using static UnityEditor.EditorGUILayout;

namespace unityEssentials.peek.options
{
    public static class UnityEssentialsPreferences
    {
        public const string SHORT_NAME_PREFERENCE   = "Peek n scroll";

        private const string MAX_SELECTED_OBJECT_STORED = SHORT_NAME_PREFERENCE + " " + "Max Selected Object stored";
        public static int GetMaxSelectedObjectStored() { return (Mathf.Max(2, GetInt(MAX_SELECTED_OBJECT_STORED, 1000))); }

        private const string MAX_SELECTED_OBJECT_SHOWN = SHORT_NAME_PREFERENCE + " " + "Max Selected Object shown";
        public static int GetMaxObjectSHown() { return (Mathf.Max(2, GetInt(MAX_SELECTED_OBJECT_SHOWN, 100))); }

        public const string MAX_PINNED_OBJECT = SHORT_NAME_PREFERENCE + " " + "Max Pinned Object";
        public static int GetMaxPinnedObject() { return (Mathf.Max(2, GetInt(MAX_PINNED_OBJECT, 20))); }

        public const string SHOW_PEEK_MENU          = SHORT_NAME_PREFERENCE + " " + "Show Peek menu";
        public const string POSITION_IN_TOOLBAR     = SHORT_NAME_PREFERENCE + " " + "Position In ToolBar";
        public const string SHOW_GAMEOBJECTS_FROM_OTHER_SCENE = SHORT_NAME_PREFERENCE + " " + "Show GameOjbects from other scenes";
        public const string FONT_SIZE_PEEK_WINDOW = SHORT_NAME_PREFERENCE + " " + "Foot size peek window";

        public const string ENABLE_CLIC_AND_SCROLL = SHORT_NAME_PREFERENCE + " " + "Automaticly move though child when ctrl scroll";

        public const string SIMPLE_OUTLINE = SHORT_NAME_PREFERENCE + " " + "Simple outline on 3D & 2D gameObject";
        public const string INTELIGENT_UI_OUTLINE = SHORT_NAME_PREFERENCE + " " + "Add additional calculation for 2D objectms";
        //public const string INTELIGENT_OUTLINE = SHORT_NAME_PREFERENCE + " " + "Do not outline transparent gameObject";
        //public const string INTELIGENT_CLIC = SHORT_NAME_PREFERENCE + " " + "Do not clic on transparent gameObject";
        //public const string ENABLE_OUTLINE = SHORT_NAME_PREFERENCE + " " + "Enable white outline on gameObject when Hold CTRL";
        public const string ENABLE_SHOW_NAME = SHORT_NAME_PREFERENCE + " " + "Enable name display of gameObject when Hold CTRL";
        public const string THICKNESS_OUTLINE = SHORT_NAME_PREFERENCE + " " + "Thickness of Outline";
        public const string FONT_OF_GAMEOBJECTS_NAME = SHORT_NAME_PREFERENCE + " " + "Font of GameObjects Name";
        public const string COLOR_OF_OUTLINE = SHORT_NAME_PREFERENCE + " " + "Color Of Outline";
        
        public const string NAME_PREFERENCE = "Project/" + SHORT_NAME_PREFERENCE;
        private const int SPACING_SECTION = 25;
        private const int PADDING_BOTTOM_SECTION = 5;

        private static Color _currentColor = Color.white;


#if UNITY_2018_3_OR_NEWER
        private static HashSet<string> SEARCH_KEYWORDS = new HashSet<string>(new[]
        { 
            "Peek",
            "Peek n scroll",
            "flow",
            "Outline",
            "gameObject",
            "link",
            "bookmarks",
            "bookmark",
            "pin"
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

        public static int GetInt(string key, int defaultValue)
        {
            if (!EditorPrefs.HasKey(key))
            {
                EditorPrefs.SetInt(key, defaultValue);
                return (defaultValue);
            }
            return (EditorPrefs.GetInt(key));
        }
        public static float GetFloat(string key, float defaultValue)
        {
            if (!EditorPrefs.HasKey(key))
            {
                EditorPrefs.SetFloat(key, defaultValue);
                return (defaultValue);
            }
            return (EditorPrefs.GetFloat(key));
        }
        public static bool GetBool(string key, bool defaultValue)
        {
            if (!EditorPrefs.HasKey(key))
            {
                EditorPrefs.SetBool(key, defaultValue);
                return (defaultValue);
            }
            return (EditorPrefs.GetBool(key));
        }

        /// <summary>
        /// get the current color. If this is the first time, create it
        /// </summary>
        /// <returns></returns>
        public static Color GetDefaultColor()
        {
            if (!EditorPrefs.HasKey(COLOR_OF_OUTLINE))
            {
                EditorPrefs.SetString(COLOR_OF_OUTLINE, EditorJsonUtility.ToJson(_currentColor));
                return (_currentColor);
            }
            _currentColor = (Color)JsonUtility.FromJson(EditorPrefs.GetString(COLOR_OF_OUTLINE), typeof(Color));
            return (_currentColor);
        }

        private static void OnProviderCustomGUI()
        {
            try
            {
                EditorGUI.BeginChangeCheck();
                PreferenceGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    ToolbarExtender.Repaint();
                }
            }
            catch { }
        }

        /// <summary>
        /// display inside preferences
        /// </summary>
        /// <param name="serialized"></param>
        private static void PreferenceGUI()
        {
            EditorGUILayout.HelpBox(SHORT_NAME_PREFERENCE + " Preferences", MessageType.Info);

            if (!ExtGUILayout.Section("Peek Toolbar", "Slider Icon", true, "Peek toolbar fold options", PADDING_BOTTOM_SECTION))
            {
                ManageBool(SHOW_PEEK_MENU, "Show Peek menu", "Show main toolbar menu", true);
                ManageSlider(POSITION_IN_TOOLBAR, "Position in toolbar: ", 0.05f, 0f, 1f);
                GUILayout.Space(SPACING_SECTION);
            }

            if (!ExtGUILayout.Section("Peek Window", "winbtn_win_rest_h", true, "Peek window fold options", PADDING_BOTTOM_SECTION))
            {
                EditorGUI.BeginChangeCheck();
                {
                    ManagePositiveInt(MAX_SELECTED_OBJECT_STORED, "Max selected object stored", 1000);
                    ManagePositiveInt(MAX_SELECTED_OBJECT_SHOWN, "Max selected object shown", 100);
                    ManagePositiveInt(MAX_PINNED_OBJECT, "Max pinned object", 20);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    ToolsButton.PeekTool?.PeekLogic?.ShrunkListIfNeeded();
                }
                ManageBool(SHOW_GAMEOBJECTS_FROM_OTHER_SCENE, "Show GameOjbects from other scene", "the reference may be recover if user change scene, or Undo deletion", true);
                EditorGUI.BeginChangeCheck();
                {
                    ManageSliderInt(FONT_SIZE_PEEK_WINDOW, "Font of gameObject's name: ", 14, 6, 20);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    ExtGUIStyles.MicroButtonLeftCenter.fontSize = EditorPrefs.GetInt(FONT_SIZE_PEEK_WINDOW, 14);
                }
                DisplayClearListButton();
                GUILayout.Space(SPACING_SECTION);
            }

            if (!ExtGUILayout.Section("Clic & Scroll", "AvatarPivot", true, "Clic & Scroll fold options", PADDING_BOTTOM_SECTION))
            {
                ManageBool(ENABLE_CLIC_AND_SCROLL, "Automaticly move though child when ctrl scroll", "When doing CTRL + SCROLL while pointing on a gameObject in the scene view, select elements behind", true);
                GUILayout.Space(SPACING_SECTION);
            }

            if (!ExtGUILayout.Section("Outline", "RectTool", true, "Outline fold options", PADDING_BOTTOM_SECTION))
            {
                ManageBool(SIMPLE_OUTLINE, "Simple outline", "Show outline on pointer gameObject", true);
                ManageBool(INTELIGENT_UI_OUTLINE, "Inteligent UI outline & clic", "Show outline and clic on gameObjects that are visible on screen, even if some transparent gameObjects are in front of it. It may be more cost effective", true);
                EditorGUI.BeginDisabledGroup(!EditorPrefs.GetBool(SIMPLE_OUTLINE) && !EditorPrefs.GetBool(INTELIGENT_UI_OUTLINE));
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        ManageColor(COLOR_OF_OUTLINE, "Outline Color");
                        ManageSliderInt(THICKNESS_OUTLINE, "Outline Thickness: ", 4, 0, 15);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        ShaderOutline.GetMaterials(true);
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(15);

                ManageBool(ENABLE_SHOW_NAME, "Show gameObject names on sceneView", "Show gameObject's name on pointed gameObject when Hold CTRL", true);
                EditorGUI.BeginDisabledGroup(!EditorPrefs.GetBool(ENABLE_SHOW_NAME));
                {
                    ManageSliderInt(FONT_OF_GAMEOBJECTS_NAME, "Font of gameObject's name: ", 22, 3, 60);
                }
                EditorGUI.EndDisabledGroup();

            }
        }

        private static void DisplayClearListButton()
        {
            using (HorizontalScope horizontalScope = new HorizontalScope())
            {
                GUILayout.Label("Reset bookmarks & previously selected list: ", GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                {
                    HiddedEditorWindow hidderEditorWindow = EditorWindow.GetWindow<HiddedEditorWindow>();
                    hidderEditorWindow.Reset();
                }
            }
        }

        private static void ManagePositiveInt(string key, string description, int defaultValue)
        {
            EditorGUI.BeginChangeCheck();
            int amount = EditorGUILayout.IntField(new GUIContent(description), EditorPrefs.GetInt(key, defaultValue));
            amount = Mathf.Max(0, amount);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(key, amount);
            }
        }

        private static void ManageBool(string key, string description, string tooltip, bool defaultValue)
        {
            EditorGUI.BeginChangeCheck();
            bool show = GUILayout.Toggle(EditorPrefs.GetBool(key, defaultValue), new GUIContent(description, tooltip));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(key, show);
            }
        }

        /// <summary>
        /// From a given key, add a colorField, display, and save when changes are made
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        private static void ManageColor(string key, string description)
        {
            if (!EditorPrefs.HasKey(COLOR_OF_OUTLINE))
            {
                EditorPrefs.SetString(COLOR_OF_OUTLINE, EditorJsonUtility.ToJson(_currentColor));
            }
            EditorGUI.BeginChangeCheck();
            Color color = EditorGUILayout.ColorField(new GUIContent(description), _currentColor);
            if (EditorGUI.EndChangeCheck())
            {
                _currentColor = color;
                EditorPrefs.SetString(key, EditorJsonUtility.ToJson(_currentColor));
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

        private static void ManageSliderInt(string key, string description, int defaultValue, int from, int to)
        {
            using (HorizontalScope horiz = new HorizontalScope())
            {
                EditorGUILayout.LabelField(description, GUILayout.ExpandWidth(false));

                EditorGUI.BeginChangeCheck();
                float sliderPosition = EditorGUILayout.Slider(EditorPrefs.GetInt(key, defaultValue), from, to, GUILayout.Width(130));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetInt(key, (int)sliderPosition);
                }
            }
        }


    }
}