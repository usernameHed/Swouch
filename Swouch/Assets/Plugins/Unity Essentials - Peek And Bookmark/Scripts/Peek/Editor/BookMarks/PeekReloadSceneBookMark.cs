using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using unityEssentials.peek.extensions;

namespace unityEssentials.peek
{
    public class PeekReloadSceneBookMark
    {
        private HiddedEditorWindow _hiddedEditorWindow;
        private List<Transform> _gameObjectsList = new List<Transform>(300);
        private PeekLogic _peekLogic;

        private string _sceneNameOfGameObjectToSelect;
        private string _gameObjectNametoSelect;
        private bool _mustSearchForGameObject;
        public event UnityAction<GameObject> LoadGameObjectWhenSceenOpen;

        public void Init(PeekLogic peekLogic, HiddedEditorWindow hiddedEditorWindow)
        {
            _peekLogic = peekLogic;
            _hiddedEditorWindow = hiddedEditorWindow;
            _mustSearchForGameObject = false;
        }

        public void AutomaticlySelectWhenFound(string toSelectName, string sceneName)
        {
            _gameObjectNametoSelect = toSelectName;
            _sceneNameOfGameObjectToSelect = sceneName;
            _mustSearchForGameObject = true;
        }

        /// <summary>
        /// Attempt to relink all gameObject found in current scene
        /// with bookMarks with by their [Scene name + name]
        /// </summary>
        /// <param name="scene"></param>
        public void ReloadSceneFromOneLoadedScene(Scene scene)
        {
            _gameObjectsList.Clear();
            AppendAllTransformInSceneToList(scene);
            for (int i = 0; i < _gameObjectsList.Count; i++)
            {
                LoadGameObjectWhenSceenOpen?.Invoke(_gameObjectsList[i].gameObject);
            }
            RelinkNamesWithGameObjects(_gameObjectsList);
        }

        /// <summary>
        /// Append all Transform found in this scene into _gameObjectsList list
        /// </summary>
        /// <param name="scene"></param>
        public void AppendAllTransformInSceneToList(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] allChilds = roots[i].GetComponentsInChildren<Transform>(true);
                _gameObjectsList.Append<Transform>(allChilds.ToList());
            }
        }

        /// <summary>
        /// Attempt to relink all gameObject
        /// with bookMarks with by their [Scene name + name]
        /// 
        /// Do a deep clean of list before!
        /// </summary>
        /// <param name="foundObjects"></param>
        public void RelinkNamesWithGameObjects(List<Transform> foundObjects)
        {
            if (foundObjects == null || _hiddedEditorWindow == null)
            {
                return;
            }
            CleanArraysFromDuplicateInNameAndObjectReferences(ref _hiddedEditorWindow.PinnedObjectsInScenes, ref _hiddedEditorWindow.PinnedObjectsNameInScenes, ref _hiddedEditorWindow.PinnedObjectsScenesLink);
            for (int i = 0; i < _hiddedEditorWindow.PinnedObjectsInScenes.Count; i++)
            {
                if (_hiddedEditorWindow.PinnedObjectsInScenes[i] == null)
                {
                    for (int k = 0; k < foundObjects.Count; k++)
                    {
                        GameObject foundGameObject = foundObjects[k].gameObject;
                        if (string.Equals(
                            _hiddedEditorWindow.PinnedObjectsScenesLink[i].name + "/" + _hiddedEditorWindow.PinnedObjectsNameInScenes[i],
                            foundGameObject.scene.name + "/" + foundGameObject.name))
                        {
                            _hiddedEditorWindow.PinnedObjectsInScenes[i] = foundGameObject;
                            if (_mustSearchForGameObject
                                && string.Equals(foundGameObject.scene.name, _sceneNameOfGameObjectToSelect)
                                && string.Equals(foundGameObject.name, _gameObjectNametoSelect))
                            {
                                _mustSearchForGameObject = false;
                                _peekLogic.ForceSelection(foundGameObject);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deep clean of list, here an exemple: (see CleanTestFunction)
        /// list:
        ///     [NAME]  : [OBJ]
        /// 0   Yellow  : Object B      <- Process: Remove 3 & 7 (same object)
        /// 1   Yellow  : Object A      <- Process: Remove 2 (is null, not 4 because different Object)
        /// 2̶ ̶ ̶ ̶Y̶e̶l̶l̶o̶w̶ ̶ ̶:̶ ̶n̶u̶l̶l̶
        /// 3̶ ̶ ̶ ̶Y̶e̶l̶l̶o̶w̶ ̶ ̶:̶ ̶O̶b̶j̶e̶c̶t̶ ̶B̶
        /// 4   Yellow  : Object C      <- Process:
        /// 5   Red     : null          <- Process: Remove 6 (same name, both null)
        /// 6̶ ̶ ̶ ̶R̶e̶d̶ ̶ ̶ ̶ ̶ ̶:̶ ̶n̶u̶l̶l̶
        /// 7 ̶ ̶ ̶R̶e̶d̶ ̶ ̶ ̶ ̶ ̶:̶ ̶O̶b̶j̶e̶c̶t̶ ̶B̶
        /// 8̶ ̶ ̶ ̶G̶r̶e̶e̶n̶ ̶ ̶ ̶:̶ ̶n̶u̶l̶l̶ ̶ ̶ ̶ ̶ ̶     <- Process: remove itself 8 (same name, other has Object) break k loop
        /// 9   Green   : Object K      <- Process: 
        /// 10  Black   : Object Z      <- Process:
        /// </summary>
        public static void CleanArraysFromDuplicateInNameAndObjectReferences(ref List<UnityEngine.Object> objectReferences, ref List<string> pathObjects, ref List<UnityEngine.Object> sceneLinkedReference)
        {
            if (objectReferences == null || pathObjects == null || objectReferences.Count != pathObjects.Count || objectReferences.Count == 0)
            {
                return;
            }

            int countArray = objectReferences.Count;
            for (int i = countArray - 1; i >= 0; i--)
            {
                if (i >= objectReferences.Count || i < 0)
                {
                    continue;
                }

                UnityEngine.Object current = objectReferences[i];
                string currentName = pathObjects[i];

                if (string.IsNullOrEmpty(currentName))
                {
                    objectReferences.RemoveAt(i);
                    pathObjects.RemoveAt(i);
                    sceneLinkedReference.RemoveAt(i);
                    continue;
                }
                currentName = sceneLinkedReference[i] + "/" + currentName;

                for (int k = i - 1; k >= 0; k--)
                {
                    UnityEngine.Object toTest = objectReferences[k];
                    string toTestName = sceneLinkedReference[k] + "/" + pathObjects[k];

                    if (Equals(current, toTest) && current != null && toTest != null)
                    {
                        objectReferences.RemoveAt(k);
                        pathObjects.RemoveAt(k);
                        sceneLinkedReference.RemoveAt(k);
                    } //1: Object A     2: Object A
                    else if (Equals(currentName, toTestName))                           //1: name A        2: name A
                    {
                        if (current != null && toTest == null)                                  //1: Object A      2: null
                        {
                            objectReferences.RemoveAt(k);
                            pathObjects.RemoveAt(k);
                            sceneLinkedReference.RemoveAt(k);
                            i--;
                        }
                        else if (current != null && toTest != null)                             //1: Object A       2: Object B
                        {
                            //nothing
                        }
                        else if (current == null && toTest != null)                             //1: null           2: ObjectA
                        {
                            objectReferences.RemoveAt(i);
                            pathObjects.RemoveAt(i);
                            sceneLinkedReference.RemoveAt(i);
                            break;
                        }
                        else if (current == null && toTest == null)                             //1: null           2: null
                        {
                            objectReferences.RemoveAt(k);
                            pathObjects.RemoveAt(k);
                            sceneLinkedReference.RemoveAt(k);
                            i--;
                        }
                    }
                }
            }
        }
        //end of class
    }
}