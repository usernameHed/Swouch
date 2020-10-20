using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using unityEssentials.sceneWorkflow.extension;
using unityEssentials.sceneWorkflow.extensions.editor;
using static UnityEditor.EditorGUILayout;

namespace unityEssentials.sceneWorkflow.toolbarExtent
{
    public class SceneWorkflowButtons
    {
        private const int HEIGHT_TEXT = 8;
        private const int HEIGHT_BUTTON = 15;
        private const int WIDTH_BUTTON = 23;
        private const int SIZE_SCENE_WORKFLOW = 120;
        public static readonly GUIStyle _miniText;

        private static ContextListerAsset _refGameAsset;

        static SceneWorkflowButtons()
        {
            _miniText = new GUIStyle()
            {
                fontSize = 9,
            };
            _miniText.normal.textColor = Color.white;
        }

        public void Init()
        {
            _refGameAsset = ExtFindEditor.GetAssetByGenericType<ContextListerAsset>();
        }

        public void DisplaySliderLeft()
        {
            if (!EditorPrefs.GetBool(UnityEssentialsPreferences.SHOW_SCENE_BUTTONS, true))
            {
                return;
            }
            float percent = EditorPrefs.GetFloat(UnityEssentialsPreferences.POSITION_IN_TOOLBAR, 0.95f);
            if (percent > 0.5f)
            {
                return;
            }
            Rect left = ToolbarExtender.GetLeftRect();
            percent = Remap(percent, 0f, 0.5f, 0f, 1f);
            float width = (left.width - SIZE_SCENE_WORKFLOW) / 1f * percent;
            GUILayout.Label("", GUILayout.MinWidth(0), GUILayout.Width(width));
            DisplaySceneForkflowButtons(width);
        }

        public void DisplaySliderRight()
        {
            if (!EditorPrefs.GetBool(UnityEssentialsPreferences.SHOW_SCENE_BUTTONS, true))
            {
                return;
            }
            float percent = EditorPrefs.GetFloat(UnityEssentialsPreferences.POSITION_IN_TOOLBAR, 0.95f);
            if (percent <= 0.5f)
            {
                return;
            }
            Rect left = ToolbarExtender.GetRightRect();
            percent = Remap(percent, 0.5f, 1f, 0f, 1f);
            float width = (left.width - SIZE_SCENE_WORKFLOW) / 1f * percent;
            GUILayout.Label("", GUILayout.MinWidth(0), GUILayout.Width(width));
            DisplaySceneForkflowButtons(width);
        }

        private void DisplaySceneForkflowButtons(float width)
        {
            if (_refGameAsset == null)
            {
                if (GUILayout.Button("create Context Referencer", ExtGUIStyles.MicroButton, GUILayout.Width(200), GUILayout.Height(HEIGHT_BUTTON)))
                {
                    _refGameAsset = ExtFindEditor.GetAssetByGenericType<ContextListerAsset>();
                    if (_refGameAsset == null)
                    {
                        _refGameAsset = GenerateContextReferencer();
                    }
                    EditorGUIUtility.PingObject(_refGameAsset);
                    Selection.activeObject = _refGameAsset;
                }
                GUILayout.FlexibleSpace();
                return;
            }



            using (VerticalScope vertical = new VerticalScope())
            {
                GUILayout.Label("Scenes Context", ExtGUIStyles.MiniTextCentered, GUILayout.Height(HEIGHT_TEXT));
                using (HorizontalScope horizontal = new HorizontalScope())
                {
                    if (GUILayout.Button("Context:", ExtGUIStyles.MicroButton, GUILayout.Width(70), GUILayout.Height(HEIGHT_BUTTON)))
                    {
                        EditorGUIUtility.PingObject(_refGameAsset);
                        Selection.activeObject = _refGameAsset;
                    }
                    int currentLoaded = _refGameAsset.LastIndexUsed;
                    for (int i = 0; i < _refGameAsset.CountSceneToLoad; i++)
                    {
                        bool currentLoadedScene = currentLoaded == i;
                        string levelIndex = (i + 1).ToString();
                        currentLoadedScene = GUILayout.Toggle(currentLoadedScene, new GUIContent(levelIndex, "Load context[" + levelIndex + "]: " + _refGameAsset.GetSceneAddet(i).NameContext), ExtGUIStyles.MicroButton, GUILayout.Width(WIDTH_BUTTON), GUILayout.Height(HEIGHT_BUTTON));
                        if (currentLoadedScene != (currentLoaded == i))
                        {
                            if (!Application.isPlaying)
                            {
                                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    return;
                                }
                            }
                            _refGameAsset.LastIndexUsed = i;
                            _refGameAsset.LoadScenesByIndex(i, OnLoadedScenes, hardReload: true);
                            EditorGUIUtility.PingObject(_refGameAsset.GetSceneAddet(i));
                            Selection.activeObject = _refGameAsset.GetSceneAddet(i);
                            return;
                        }
                    }
                }
            }
        }

        private static ContextListerAsset GenerateContextReferencer()
        {
            ContextListerAsset globalContextLister = ExtScriptableObject.CreateAsset<ContextListerAsset>("Assets/Context Lister.asset");
            SceneContextAsset context = ExtScriptableObject.CreateAsset<SceneContextAsset>("Assets/Context 1.asset");
            context.NameContext = "Demo Scene List";
            ExtSceneReference[] sceneItems = ExtSceneReference.GetAllActiveScene();
            context.SceneToLoad.Append(sceneItems.ToList());
            globalContextLister.AddContext(context);

            globalContextLister.Save();
            context.Save();
            AssetDatabase.Refresh();

            return (globalContextLister);
        }

        private void OnLoadedScenes(SceneContextAsset lister)
        {
            if (Application.isPlaying)
            {
                AbstractLinker.Instance?.InitFromPlay();
            }
            else
            {
                AbstractLinker.Instance?.InitFromEditor();
            }
        }

        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}