using UnityEditor;
using UnityEngine.SceneManagement;
using unityEssentials.peek.toolbarExtent;

namespace unityEssentials.peek
{
    [InitializeOnLoad]
    public static class BookMarkSceneEvent
    {
        static BookMarkSceneEvent()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpenedCallback;
            SceneManager.sceneLoaded += SceneOpenedPlayModeCallback;
        }

        private static void SceneOpenedPlayModeCallback(Scene scene, LoadSceneMode mode)
        {
            ToolsButton.PeekTool.PeekLogic.UpdateSceneGameObjectFromSceneLoad(scene);
        }

        private static void SceneOpenedCallback(Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            ToolsButton.PeekTool.PeekLogic.UpdateSceneGameObjectFromSceneLoad(scene);
        }
    }
}