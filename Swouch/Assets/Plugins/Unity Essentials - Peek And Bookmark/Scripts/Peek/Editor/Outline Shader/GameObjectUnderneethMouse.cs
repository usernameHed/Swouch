using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using unityEssentials.peek.extensions;
using unityEssentials.peek.extensions.editor;
using unityEssentials.peek.options;

namespace unityEssentials.peek.shaderOutline
{
    public class GameObjectUnderneethMouse
    {
        public GameObject CurrentHover { get { return (_currentHover); } }

        private const int MAX_DEPTH = 100;
        private const float TIME_BETWEEN_2_CALCUL = 0.03f;

        private bool _preventClic;
        private GameObject _currentHover;
        private MethodInfo _internalPickClosestGameObject;
        private SnapshotSceneView _snapshotSceneView = new SnapshotSceneView();
        private SceneViewChangeEvent _sceneViewChangeEvent = new SceneViewChangeEvent();
        private EditorChrono _coolDownBetween2Calcul = new EditorChrono();
        private EventType _lastCalculEvent;

        //cached datas
        private List<GameObject> _underneethGameObjects = new List<GameObject>(MAX_DEPTH);          //list of gameObjectUnderneeth when overing stuff and doing PickClosestCalculation
        private List<GameObject> _coloredUnderneethGameObjects = new List<GameObject>(MAX_DEPTH);   //all found colored GameObjects
        private List<Scene> _sceneAlreadyCalculated = new List<Scene>(MAX_DEPTH);                   //all scenes linked to found colored gameObjects
        private List<GameObject> _alreadyCalculatedNonGraphicToIgnore = new List<GameObject>(600);  //list of all gameObject to ignore during calculation (they are setup on scene load, AND on hovering)
        private GameObject[] _ignoreArray = new GameObject[MAX_DEPTH];  //used to compact _underneethGameObjects & _alreadyCalculatedNonGraphicToIgnore list
        private Vector2 _invertedPos = new Vector2();   //cache of interted mouse position

        //when manage multiple scene
        private List<Canvas> _finalCanvasLinkedToGameObjects = new List<Canvas>(MAX_DEPTH);         //list of canvas found from each found colored gameObjects
        private List<GameObject> _finalGameObjectToPickRandomly = new List<GameObject>(MAX_DEPTH);  //final colored gameObjects with canvas with the same draw order

        public void Init()
        {
            FindMethodByReflection();
            _preventClic = false;
            _sceneViewChangeEvent.Init();
            _snapshotSceneView.Init(_sceneViewChangeEvent);
        }

        /// <summary>
        /// return true if all condition are good to calculate underneeth
        /// Calculating is expensive, we need to be as performant as possible
        /// </summary>
        /// <returns></returns>
        public bool IsConditionOkToCalculateUnderneeth()
        {
            if (!Event.current.shift && !Event.current.control)
            {
                _currentHover = null;
                return (false);
            }

            if (_coolDownBetween2Calcul.IsRunning() && Event.current.type == _lastCalculEvent)
            {
                return (false);
            }
            bool isMouseMove = Event.current.type == EventType.MouseMove;
            bool isMouseClicDown = Event.current.type == EventType.MouseDown && Event.current.button == 0;
            bool isConditionOk = isMouseMove || isMouseClicDown;

            if (isConditionOk)
            {
                _coolDownBetween2Calcul.StartChrono(TIME_BETWEEN_2_CALCUL);
                _lastCalculEvent = Event.current.type;
            }
            return (isConditionOk);
        }

        /// <summary>
        /// Save all gameObjects underneeth mouse!
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="sceneView"></param>
        public void SaveAllGameObjectUnderneethMouse(Vector2 pos, SceneView sceneView)
        {
            bool hasChanged = false;
            _alreadyCalculatedNonGraphicToIgnore.CleanNullFromList(ref hasChanged);

            _invertedPos.x = pos.x;
            _invertedPos.y = sceneView.position.height - 16 - pos.y;

            int matIndex = -1;
            _currentHover = PickObjectOnPos(sceneView.camera, ~0, _invertedPos, null, null, ref matIndex);

            if (_currentHover == null)
            {
                return;
            }
            if (!UnityEssentialsPreferences.GetBool(UnityEssentialsPreferences.INTELIGENT_UI_OUTLINE, true))
            {
                return;
            }
            _currentHover = LoopThought(_currentHover, sceneView, _invertedPos);
        }

        /// <summary>
        /// loop if conditions are good
        /// </summary>
        /// <param name="currentHover"></param>
        /// <param name="sceneView"></param>
        /// <param name="invertedPos"></param>
        /// <returns></returns>
        private GameObject LoopThought(GameObject currentHover, SceneView sceneView, Vector2 invertedPos)
        {
            bool isInMouseMove = Event.current.type == EventType.MouseMove;
            bool isInMouseDown = Event.current.type == EventType.MouseDown;
            bool mustManageMultiScene = SceneManager.sceneCount > 1;
            _coloredUnderneethGameObjects.Clear();
            _underneethGameObjects.Clear();
            _sceneAlreadyCalculated.Clear();

            bool wasNull = false;
            GameObject clickedGameObject = currentHover;
            for (int i = 0; i <= MAX_DEPTH; i++)
            {
                if (i != 0)
                {
                    int matIndex = -1;
                    _ignoreArray = ExtList.CompactToArray(_alreadyCalculatedNonGraphicToIgnore, _underneethGameObjects);
                    clickedGameObject = PickObjectOnPos(sceneView.camera, ~0, invertedPos, _ignoreArray, null, ref matIndex);
                }
                //if we found anything new, we must finish action
                if (clickedGameObject == null)
                {
                    //on multi scene, PickObjectOnPos can send null once (not twice)
                    if (wasNull)
                    {
                        return FinishAction(currentHover, isInMouseMove, isInMouseDown);
                    }
                    wasNull = true;
                    continue;
                }
                wasNull = false;

                _underneethGameObjects.Add(clickedGameObject);

                //if the current gameObject scene is in the stack, that's mean we already found
                //a colored gameObject in that scene, we can go on
                if (_sceneAlreadyCalculated.Contains(clickedGameObject.scene))
                {
                    continue;
                }


                //here continue if the current one is:
                // - does not have rectTransform component
                // - does not have graphic component
                // - is transparent
                // - is disabled
                // - is inside disabled canvas
                if (MustResearchFurther(clickedGameObject, sceneView))
                {
                    continue;
                }

                //if we do not have multi-sceen to manage,
                //the first colored gameObject found is enought
                if (!mustManageMultiScene)
                {
                    currentHover = clickedGameObject;
                    return (FinishAction(currentHover, isInMouseMove, isInMouseDown));
                }

                //here we found something with color!
                //add it to the list, and continue (because they is several scene...)
                _coloredUnderneethGameObjects.Add(clickedGameObject);
                if (_sceneAlreadyCalculated.AddIfNotContain(clickedGameObject.scene))
                {
                    //Debug.Log("found " + clickedGameObject.name + " for scene :" + clickedGameObject.scene.name);
                    //Debug.Log("here place every gameObject of this scene (exept clickedGameObject) to the ignore list !");
                }
                currentHover = clickedGameObject;
            }
            return (currentHover);
        }

        private GameObject FinishAction(GameObject currentHover, bool isInMouseMove, bool isInMouseDown)
        {
            if (_coloredUnderneethGameObjects.Count > 1)
            {
                currentHover = ChooseGameObjectFromDifferentScene(_coloredUnderneethGameObjects);
            }
            ManageClicInput(currentHover, isInMouseDown, isInMouseMove);
            return (currentHover);
        }

        private GameObject ChooseGameObjectFromDifferentScene(List<GameObject> gameObjectInDifferentScene)
        {
            _finalCanvasLinkedToGameObjects.Clear();
            Canvas closestCanvas = gameObjectInDifferentScene[0].GetComponentInParent<Canvas>();
            _finalCanvasLinkedToGameObjects.Add(closestCanvas);

            int order = closestCanvas.sortingOrder;
            for (int i = 1; i < gameObjectInDifferentScene.Count; i++)
            {
                closestCanvas = gameObjectInDifferentScene[i].GetComponentInParent<Canvas>();
                if (closestCanvas.sortingOrder > order)
                {
                    order = closestCanvas.sortingOrder;
                }
                _finalCanvasLinkedToGameObjects.Add(closestCanvas);
            }
            _finalGameObjectToPickRandomly.Clear();
            for (int i = 0; i < gameObjectInDifferentScene.Count; i++)
            {
                if (_finalCanvasLinkedToGameObjects[i].sortingOrder == order)
                {
                    _finalGameObjectToPickRandomly.Add(gameObjectInDifferentScene[i]);
                }
            }
            return (_finalGameObjectToPickRandomly.PickRandom());
        }

        private void ManageClicInput(GameObject currentHover, bool isInMouseDown, bool isInMouseMove)
        {
            if (isInMouseDown && !Selection.instanceIDs.Contains(currentHover.GetInstanceID()))
            {
                if (GUIUtility.hotControl == 0)
                {
                    return;
                }
                _preventClic = true;

                if (Event.current.control)
                {
                    int[] currents = new int[Selection.instanceIDs.Length + 1];
                    for (int k = 0; k < Selection.instanceIDs.Length; k++)
                    {
                        currents[k] = Selection.instanceIDs[k];
                    }
                    currents[Selection.instanceIDs.Length] = currentHover.GetInstanceID();
                    Selection.instanceIDs = currents;
                }
                else
                {
                    Selection.activeGameObject = currentHover;
                }

            }
            else if (Event.current.control && isInMouseDown && Selection.instanceIDs.Length > 1 && Selection.instanceIDs.Contains(currentHover.GetInstanceID()))
            {
                //Event.current.Use();
                if (GUIUtility.hotControl == 0)
                {
                    return;
                }
                _preventClic = true;

                int[] currents = new int[Selection.instanceIDs.Length - 1];
                int kNewIndex = 0;
                for (int k = 0; k < Selection.instanceIDs.Length; k++)
                {
                    if (Selection.instanceIDs[k] != currentHover.GetInstanceID())
                    {
                        currents[kNewIndex] = Selection.instanceIDs[k];
                        kNewIndex++;
                    }
                }
                Selection.instanceIDs = currents;
            }
            else if (Event.current.shift && isInMouseDown)
            {
                _preventClic = true;
                Selection.instanceIDs = new int[1] { currentHover.GetInstanceID() };
            }
        }

        private bool MustResearchFurther(GameObject toTest, SceneView sceneView)
        {
            RectTransform rectTransform = toTest.GetComponent<RectTransform>();
            if (rectTransform)
            {
                Graphic graphic = toTest.GetComponent<Graphic>();
                if (graphic == null
                    //|| graphic.mainTexture == null //too expensive
                    || graphic.color.a < 0.1f
                    || !graphic.enabled)
                {
                    _alreadyCalculatedNonGraphicToIgnore.Add(toTest);
                    return (true);
                }
                if (!graphic.gameObject.activeInHierarchy)
                {
                    return (true);
                }
                Canvas[] canvas = toTest.GetComponentsInParent<Canvas>();
                for (int i = 0; i < canvas.Length; i++)
                {
                    if (!canvas[i].enabled)
                    {
                        _alreadyCalculatedNonGraphicToIgnore.Add(canvas[i].gameObject);
                        return (true);
                    }
                }
                return (!_snapshotSceneView.IsHoverTransparentPixel<Graphic>(sceneView, graphic));
            }
            _alreadyCalculatedNonGraphicToIgnore.Add(toTest);
            return (false);
        }

        public void AttemptToAddGameObjectToIgnoreList(GameObject toTest)
        {
            if (!UnityEssentialsPreferences.GetBool(UnityEssentialsPreferences.INTELIGENT_UI_OUTLINE, true))
            {
                return;
            }

            RectTransform rectTransform = toTest.GetComponent<RectTransform>();
            if (rectTransform)
            {
                Graphic graphic = toTest.GetComponent<Graphic>();
                if (graphic == null
                    //|| graphic.mainTexture == null    //Too expensive!
                    || graphic.color.a < 0.1f
                    || !graphic.enabled)
                {
                    _alreadyCalculatedNonGraphicToIgnore.Add(toTest);
                    return;
                }
                if (!graphic.gameObject.activeInHierarchy)
                {
                    return;
                }

                Canvas[] canvas = toTest.GetComponentsInParent<Canvas>();
                for (int i = 0; i < canvas.Length; i++)
                {
                    if (!canvas[i].enabled)
                    {
                        _alreadyCalculatedNonGraphicToIgnore.Add(canvas[i].gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// PICK A GAMEOBJECT FROM SCENE VIEW AT POSITION
        /// pick a gameObject from the sceneView at a given mouse position
        /// </summary>
        /// <param name="cam">current camera</param>
        /// <param name="layers">layer accepted</param>
        /// <param name="position">mouse position</param>
        /// <param name="ignore">ignored gameObjects</param>
        /// <param name="filter"></param>
        /// <param name="materialIndex"></param>
        /// <returns></returns>
        private GameObject PickObjectOnPos(Camera cam, int layers, Vector2 position, GameObject[] ignore, GameObject[] filter, ref int materialIndex)
        {
            materialIndex = -1;
            if (_internalPickClosestGameObject == null)
            {
                FindMethodByReflection();
                return (null);
            }
            return (GameObject)_internalPickClosestGameObject.Invoke(null, new object[] { cam, layers, position, ignore, filter, materialIndex });
        }

        /// <summary>
        /// Save reference of the Internal_PickClosestGO reflection method
        /// </summary>
        private void FindMethodByReflection()
        {
            Assembly editorAssembly = typeof(Editor).Assembly;
            System.Type handleUtilityType = editorAssembly.GetType("UnityEditor.HandleUtility");
            _internalPickClosestGameObject = handleUtilityType.GetMethod("Internal_PickClosestGO", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public bool ApplyPeventClic()
        {
            if (!_preventClic)
            {
                return (false);
            }
            Event.current.Use();
            GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = 0;
            _preventClic = false;
            return (true);
        }
    }
}