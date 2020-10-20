using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using unityEssentials.peek.extensions.editor.editorWindow;
using unityEssentials.peek.shaderOutline;
using unityEssentials.peek.toolbarExtent;

namespace unityEssentials.peek
{
    /// <summary>
    /// this editor window is always present, and hidded, to save
    /// every serialize objects present in the project.
    /// 
    /// This editorWindow contain the visible PeekEditorWindow reference,
    /// and the list of all selected objects.
    /// </summary>
    public class HiddedEditorWindow : DecoratorEditorWindow
    {
        public List<UnityEngine.Object> SelectedObjects = new List<UnityEngine.Object>(1000);
        public UnityEngine.Object LastSelectedObject;
        public List<UnityEngine.Object> SelectedObjectsWithoutDuplicate = new List<UnityEngine.Object>(100);
        public List<UnityEngine.Object> PinnedObjects = new List<UnityEngine.Object>(100);
        public List<UnityEngine.Object> PinnedObjectsInScenes = new List<UnityEngine.Object>(100);
        public List<string> PinnedObjectsNameInScenes = new List<string>(100);
        public List<UnityEngine.Object> PinnedObjectsScenesLink = new List<UnityEngine.Object>(100);

        public int CurrentIndex;

        private const string KEY_EDITOR_PREF_SAVE_LAST_SELECTION_CLOSED = "KEY_EDITOR_PREF_SAVE_LAST_SELECTION_CLOSED";
        private PeekEditorWindow _displayInside = null;
        public bool IsClosed { get { return (_displayInside == null); } }

        private static Rect _positionHidedEditorWindow = new Rect(10000, 10000, 0, 0);

        /// <summary>
        /// override it with "new" keyword
        /// </summary>
        public static HiddedEditorWindow ConstructHiddedWindow()
        {
            HiddedEditorWindow window = EditorWindow.GetWindow<HiddedEditorWindow>("", false, typeof(SceneView));
            window.InitConstructor();
            window.SetMinSize(new Vector2(0, 0));
            window.SetMaxSize(new Vector2(0, 0));
            window.position = _positionHidedEditorWindow;
            return (window);
        }

        public bool IsContainingThisType<T>(out UnityEngine.Object found)
        {
            found = null;

            for (int i = SelectedObjects.Count - 1; i >= 0; i--)
            {
                if (SelectedObjects[i] == null)
                {
                    continue;
                }
                if (typeof(T).IsAssignableFrom(SelectedObjects[i].GetType()))
                {
                    found = SelectedObjects[i];
                    return (true);
                }
            }
            return (false);
        }

        public void ToggleOpenCloseDisplay()
        {
            if (_displayInside)
            {
                _displayInside.Close();
                _displayInside = null;
            }
            else
            {
                _displayInside = EditorWindow.GetWindow<PeekEditorWindow>(PeekEditorWindowDrawer.PEEK_WINDOW, true);
                _displayInside.Init();
                _displayInside.Show();
            }
            SaveCloseStatus();
        }

        public void RelinkPeekWindow()
        {
            if (_displayInside)
            {
                return;
            }
            _displayInside = EditorWindow.GetWindow<PeekEditorWindow>(PeekEditorWindowDrawer.PEEK_WINDOW, true);
        }

        public void UpdatePeekEditorWindowIfOpen()
        {
            bool hasUpdatedEditorWindow = false;
            if (_displayInside == null)
            {
                _displayInside = EditorWindow.GetWindow<PeekEditorWindow>(PeekEditorWindowDrawer.PEEK_WINDOW);
                _displayInside.Init();
                SaveCloseStatus();
                hasUpdatedEditorWindow = true;
            }
            _displayInside.Show();

            if (Application.isPlaying)
            {
                ToolsButton.PeekTool.PeekLogic.UpdateSceneGameOnPlay();
            }
            else if (hasUpdatedEditorWindow)
            {
                ToolsButton.PeekTool.PeekLogic.UpdateSceneGameObjectOnLoad();
                ShaderOutline.GetMaterials(true);
            }
        }

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            ToolsButton.PeekTool.PeekLogic.UpdateSceneGameObjectOnLoad();
        }

        public void Display(Action action)
        {
            _displayInside?.ShowEditorWindow(action);
        }

        public EditorWindow GetPeekNScrollEditorWindow()
        {
            return _displayInside;
        }

        public bool WasOpenLastTimeWeCloseUnity()
        {
            return (EditorPrefs.GetBool(KEY_EDITOR_PREF_SAVE_LAST_SELECTION_CLOSED));
        }

        public void SaveCloseStatus()
        {
            EditorPrefs.SetBool(KEY_EDITOR_PREF_SAVE_LAST_SELECTION_CLOSED, _displayInside == null);
        }

        public void Reset()
        {
            SelectedObjects.Clear();
            LastSelectedObject = null;
            SelectedObjectsWithoutDuplicate.Clear();
            PinnedObjects.Clear();
            PinnedObjectsInScenes.Clear();
            PinnedObjectsNameInScenes.Clear();
            PinnedObjectsScenesLink.Clear();
            CurrentIndex = -1;
        }

        public void KeepLinkAndNamesUpToDate()
        {
            if (PinnedObjectsInScenes == null || PinnedObjectsNameInScenes == null || PinnedObjectsScenesLink == null)
            {
                return;
            }
            for (int i = 0; i < PinnedObjectsInScenes.Count; i++)
            {
                if (PinnedObjectsInScenes[i] == null)
                {
                    continue;
                }
                PinnedObjectsNameInScenes[i] = PinnedObjectsInScenes[i].name;
            }
        }

        /// <summary>
        /// trigger when scene is deleted, or on peekLogic init / update
        /// </summary>
        public void DeleteBookMarkedGameObjectOfLostScene()
        {
            for (int i = PinnedObjectsScenesLink.Count - 1; i >= 0; i--)
            {
                if (PinnedObjectsScenesLink[i] == null && PinnedObjectsInScenes[i] == null)
                {
                    PinnedObjectsScenesLink.RemoveAt(i);
                    PinnedObjectsInScenes.RemoveAt(i);
                    PinnedObjectsNameInScenes.RemoveAt(i);
                }
            }
        }
    }
}