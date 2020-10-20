using System;
using UnityEditor;
using UnityEngine;
using unityEssentials.peek.extensions.editor;
using unityEssentials.peek.options;
using unityEssentials.peek.toolbarExtent;

namespace unityEssentials.peek.shaderOutline
{
    /// <summary>
    /// this class is only used to display an outline (and manage clic if necessary)
    /// </summary>
    [InitializeOnLoad]
    public class ShaderOutline
    {
        private static bool _canShowOutlineFor3d;
        private static bool _canShowOutlineForUI;

        private static Material _transparent;
        private static Material _transparentSprite;
        private static Material _outlineMesh;
        private static Material _outlineSprite;
        private static Material _outlineUI;
        private static Material _outlineMask;

        private static GameObject _selectedByClicAndScroll;
        private static GameObjectUnderneethMouse _underneethMouse = new GameObjectUnderneethMouse();

        static ShaderOutline()
        {
            try
            {
#if UNITY_2018_4_OR_NEWER
                SceneView.duringSceneGui += OnSceneGUI;
#else
                SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
                Selection.selectionChanged += OnSelectionChanged;
                if (UnityEssentialsPreferences.GetBool(UnityEssentialsPreferences.INTELIGENT_UI_OUTLINE, true))
                {
                    ToolsButton.PeekTool.PeekLogic.PeekReloadBookMark.LoadGameObjectWhenSceenOpen += _underneethMouse.AttemptToAddGameObjectToIgnoreList;
                }
                GetMaterials(true);
                _underneethMouse.Init();
            }
            catch { }
        }

        /// <summary>
        /// hide outline on 3d object when selection changed
        /// </summary>
        private static void OnSelectionChanged()
        {
            _canShowOutlineFor3d = false;
        }

        /// <summary>
        /// called by ClicAndScroll extension
        /// </summary>
        public static void SelectionChangedByClicAndScroll()
        {
            _selectedByClicAndScroll = Selection.activeGameObject;
        }

        /// <summary>
        /// instantiate tmp materials
        /// </summary>
        /// <param name="force"></param>
        public static void GetMaterials(bool force)
        {
            if (_transparent.IsTruelyNull() || force)
            {
                _transparent = GameObject.Instantiate(new Material(Shader.Find("Sprites/Default")));
                _transparent.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
            }
            if (_transparentSprite.IsTruelyNull() || force)
            {
                _transparentSprite = GameObject.Instantiate(new Material(Shader.Find("Sprites/Default")));
                _transparentSprite.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
            }
            if (_outlineMesh.IsTruelyNull() || force)
            {
                _outlineMesh = GameObject.Instantiate(new Material(Shader.Find("Peek/Outline Fill")));
                _outlineMesh.SetColor("_OutlineColor", UnityEssentialsPreferences.GetDefaultColor());
                _outlineMesh.SetFloat("_OutlineWidth", UnityEssentialsPreferences.GetInt(UnityEssentialsPreferences.THICKNESS_OUTLINE, 4));
            }
            if (_outlineSprite.IsTruelyNull() || force)
            {
                _outlineSprite = GameObject.Instantiate(new Material(Shader.Find("Peek/Outline Fill")));
                _outlineSprite.SetColor("_OutlineColor", UnityEssentialsPreferences.GetDefaultColor());
                _outlineSprite.SetFloat("_OutlineWidth", UnityEssentialsPreferences.GetInt(UnityEssentialsPreferences.THICKNESS_OUTLINE, 4));
            }
            if (_outlineUI.IsTruelyNull() || force)
            {
                _outlineUI = GameObject.Instantiate(new Material(Shader.Find("Peek/Outline Fill")));
                _outlineUI.SetColor("_OutlineColor", UnityEssentialsPreferences.GetDefaultColor());
                _outlineUI.SetFloat("_OutlineWidth", UnityEssentialsPreferences.GetInt(UnityEssentialsPreferences.THICKNESS_OUTLINE, 4));
            }
            if (_outlineMask.IsTruelyNull() || force)
            {
                _outlineMask = GameObject.Instantiate(new Material(Shader.Find("Peek/Outline Mask")));
            }
        }

        /// <summary>
        /// Do outline & clic action on sceneView
        /// </summary>
        /// <param name="sceneView"></param>
        private static void OnSceneGUI(SceneView sceneView)
        {
            try
            {
                OnCustomGUI(sceneView);
            }
            catch { }
        }

        private static void OnCustomGUI(SceneView sceneView)
        {
            if (Event.current.alt)
            {
                return;
            }
            if (Tools.current != Tool.Move && Tools.current != Tool.Rect)
            {
                return;
            }

            if (!Event.current.shift && !Event.current.control)
            {
                _selectedByClicAndScroll = null;
                return;
            }
            bool outlineEnabledByUser = UnityEssentialsPreferences.GetBool(UnityEssentialsPreferences.SIMPLE_OUTLINE, true) || UnityEssentialsPreferences.GetBool(UnityEssentialsPreferences.INTELIGENT_UI_OUTLINE, true);
            bool namesEnabledByUser = UnityEssentialsPreferences.GetBool(UnityEssentialsPreferences.ENABLE_SHOW_NAME, true);

            if (!outlineEnabledByUser && !namesEnabledByUser)
            {
                return;
            }

            if (_underneethMouse.IsConditionOkToCalculateUnderneeth())
            {
                _underneethMouse.SaveAllGameObjectUnderneethMouse(Event.current.mousePosition, sceneView);
                _canShowOutlineFor3d = true;
                if (Selection.activeGameObject == _underneethMouse.CurrentHover)
                {
                    _canShowOutlineFor3d = false;
                }
                _canShowOutlineForUI = true;
            }
            
            if (_underneethMouse.ApplyPeventClic())
            {
                return;
            }            

            //if no gameObject found under mouth, do nothing
            if (_underneethMouse.CurrentHover == null)
            {
                return;
            }

            //Finally apply outline & names
            bool isObjectUI = false;
            if (outlineEnabledByUser && (Event.current.shift || Event.current.control))
            {
                ManageOutline(sceneView, ref isObjectUI);
            }
            if (namesEnabledByUser && (Event.current.shift || Event.current.control))
            {
                if (!_canShowOutlineForUI && isObjectUI)
                {
                    return;
                }

                if ((_canShowOutlineFor3d || !_canShowOutlineFor3d && Selection.activeObject == _underneethMouse.CurrentHover))
                {
                    DisplayStringInSceneViewFrom2dPosition(Event.current.mousePosition, _underneethMouse.CurrentHover.name, Color.white, UnityEssentialsPreferences.GetInt(UnityEssentialsPreferences.FONT_OF_GAMEOBJECTS_NAME, 22));
                }
                else if (_selectedByClicAndScroll != null && Selection.activeObject == _selectedByClicAndScroll)
                {
                    DisplayStringInSceneViewFrom3dPosition(_selectedByClicAndScroll.transform.position, _selectedByClicAndScroll.name, Color.white, UnityEssentialsPreferences.GetInt(UnityEssentialsPreferences.FONT_OF_GAMEOBJECTS_NAME, 22));
                }
            }
        }

        /// <summary>
        /// apply outline mesh depending on the type of component
        /// a RectTransform need a Quad outline on world position on top of UI
        /// a Mesh need a Mesh outline
        /// a SpriteRender need a special sprite to mesh outline
        /// - TrailRenderer, SkinnedMeshRenderer & other miscellaneous render
        ///     are not supported at the moment
        /// </summary>
        /// <param name="sceneView"></param>
        /// <param name="isObjectUI"></param>
        private static void ManageOutline(SceneView sceneView, ref bool isObjectUI)
        {
            GetMaterials(false);

            RectTransform rectTransform = _underneethMouse.CurrentHover.GetComponent<RectTransform>();
            if (rectTransform)
            {
                isObjectUI = true;
                if (_canShowOutlineForUI)
                {
                    ManageOutlineForUI(rectTransform, sceneView);
                }
                return;
            }
            SpriteRenderer spriteRenderer = _underneethMouse.CurrentHover.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                bool canShowSprite = (_canShowOutlineFor3d || !_canShowOutlineFor3d && Selection.activeGameObject == _underneethMouse.CurrentHover);
                if (canShowSprite)
                {
                    ManageOutlineForSprite(spriteRenderer, sceneView);
                }
                return;
            }

            if (!_canShowOutlineFor3d)
            {
                return;
            }
            MeshFilter meshFilter = _underneethMouse.CurrentHover.GetComponent<MeshFilter>();
            if (meshFilter)
            {
                ManageOutlineMeshFilter(meshFilter, sceneView);
            }
        }

        /// <summary>
        /// from a meshFilter, draw a mesh outline
        /// </summary>
        /// <param name="meshFilter"></param>
        /// <param name="sceneView"></param>
        private static void ManageOutlineMeshFilter(MeshFilter meshFilter, SceneView sceneView)
        {
            Mesh mesh = meshFilter.sharedMesh;
            DrawMesh(sceneView, mesh, _transparent, _outlineMesh, _outlineMask);
        }

        /// <summary>
        /// From a RectTransform, draw a quad outline
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="sceneView"></param>
        private static void ManageOutlineForUI(RectTransform rectTransform, SceneView sceneView)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            DrawQuad(sceneView, corners, _transparent, _outlineUI, _outlineMask);
        }

        /// <summary>
        /// draw a quad (3 borders)
        /// </summary>
        /// <param name="sceneView"></param>
        /// <param name="corner"></param>
        /// <param name="mat1"></param>
        /// <param name="mat2"></param>
        /// <param name="mat3"></param>
        private static void DrawQuad(SceneView sceneView, Vector3[] corner, Material mat1, Material mat2, Material mat3)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Mesh quad1 = BuildQuad(corner, Vector3.up + Vector3.right);
                Mesh quad2 = BuildQuad(corner, Vector3.down + Vector3.left);

                Graphics.DrawMesh(quad1, -Vector3.forward * 0.1f, Quaternion.identity, mat1, 0, sceneView.camera);
                Graphics.DrawMesh(quad1, -Vector3.forward * 0.1f, Quaternion.identity, mat2, 0, sceneView.camera);
                Graphics.DrawMesh(quad1, -Vector3.forward * 0.1f, Quaternion.identity, mat3, 0, sceneView.camera);
                Graphics.DrawMesh(quad2, -Vector3.forward * 0.1f, Quaternion.identity, mat1, 0, sceneView.camera);
                Graphics.DrawMesh(quad2, -Vector3.forward * 0.1f, Quaternion.identity, mat2, 0, sceneView.camera);
                Graphics.DrawMesh(quad2, -Vector3.forward * 0.1f, Quaternion.identity, mat3, 0, sceneView.camera);
            }
        }

        /// <summary>
        /// build quad mesh from corders & normals
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="normals"></param>
        /// <returns></returns>
        private static Mesh BuildQuad(Vector3[] corner, Vector3 normals)
        {
            Mesh mesh = new Mesh();

            // Setup vertices
            Vector3[] newVertices = new Vector3[4];
            newVertices[0] = corner[0];
            newVertices[1] = corner[3];
            newVertices[2] = corner[1];
            newVertices[3] = corner[2];

            // Setup UVs
            Vector2[] newUVs = new Vector2[newVertices.Length];
            newUVs[0] = new Vector2(0, 0);
            newUVs[1] = new Vector2(0, 1);
            newUVs[2] = new Vector2(1, 0);
            newUVs[3] = new Vector2(1, 1);

            // Setup triangles
            int[] newTriangles = new int[] { 0, 1, 2, 3, 2, 1 };

            // Setup normals
            Vector3[] newNormals = new Vector3[newVertices.Length];
            newNormals[0] = normals;
            newNormals[1] = normals;
            newNormals[2] = normals;
            newNormals[3] = normals;

            // Create quad
            mesh.vertices = newVertices;
            mesh.uv = newUVs;
            mesh.triangles = newTriangles;
            mesh.normals = newNormals;

            return (mesh);
        }
        
        /// <summary>
        /// Draw a mesh outline from a sprite (convert sprite to mesh)
        /// </summary>
        /// <param name="spriteRenderer"></param>
        /// <param name="sceneView"></param>
        private static void ManageOutlineForSprite(SpriteRenderer spriteRenderer, SceneView sceneView)
        {
            if (spriteRenderer.sprite == null)
            {
                return;
            }
            Mesh mesh = SpriteToMesh(spriteRenderer.sprite);
            DrawMesh(sceneView, mesh, _transparentSprite, _outlineSprite, _outlineMask);
        }

        /// <summary>
        /// convert sprite to mesh
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        private static Mesh SpriteToMesh(Sprite sprite)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = Array.ConvertAll(sprite.vertices, i => (Vector3)i);
            mesh.uv = sprite.uv;
            mesh.triangles = Array.ConvertAll(sprite.triangles, i => (int)i);
            
            mesh.RecalculateNormals();
            Vector3[] newNormals = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < mesh.normals.Length; i++)
            {
                newNormals[i] = mesh.vertices[i];
            }
            mesh.normals = newNormals;
            return mesh;
        }

        /// <summary>
        /// draw mesh & materials
        /// </summary>
        /// <param name="sceneView"></param>
        /// <param name="mesh"></param>
        /// <param name="mat1"></param>
        /// <param name="mat2"></param>
        /// <param name="mat3"></param>
        private static void DrawMesh(SceneView sceneView, Mesh mesh, Material mat1, Material mat2, Material mat3)
        {
            Vector3 position = _underneethMouse.CurrentHover.transform.position;
            Quaternion rotation = _underneethMouse.CurrentHover.transform.rotation;
            Vector3 scale = _underneethMouse.CurrentHover.transform.lossyScale;

            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

            if (Event.current.type == EventType.Repaint)
            {
                Graphics.DrawMesh(mesh, matrix, mat1, 0, sceneView.camera);
                Graphics.DrawMesh(mesh, matrix, mat2, 0, sceneView.camera);
                Graphics.DrawMesh(mesh, matrix, mat3, 0, sceneView.camera);
            }
        }

        /// <summary>
        /// display string in sceneView from 2d position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="toDisplay"></param>
        public static void DisplayStringInSceneViewFrom2dPosition(Vector2 position, string toDisplay, Color color, int fontSize = 20)
        {
            GUIStyle textStyle = new GUIStyle();
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = color;
            textStyle.fontStyle = FontStyle.Bold;

            GUIStyle textStyleThickness = new GUIStyle(textStyle);
            textStyleThickness.normal.textColor = Color.black;

            Handles.BeginGUI();
            GUI.Label(new Rect(position.x + 5f + 2f, position.y - 17f + 2f, 10000, EditorGUIUtility.singleLineHeight),
                new GUIContent(toDisplay),
                textStyleThickness);
            GUI.Label(new Rect(position.x + 5f, position.y - 17f, 10000, EditorGUIUtility.singleLineHeight),
                new GUIContent(toDisplay),
                textStyle);
            Handles.EndGUI();
        }

        /// <summary>
        /// display string in sceneView from 3d position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="toDisplay"></param>
        public static void DisplayStringInSceneViewFrom3dPosition(Vector3 position, string toDisplay, Color color, int fontSize = 20)
        {
            GUIStyle textStyle = new GUIStyle();
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = color;
            textStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle textStyleThickness = new GUIStyle(textStyle);
            textStyleThickness.normal.textColor = Color.black;

            Handles.Label(position + new Vector3(3f, -3f, 0), toDisplay, textStyleThickness);
            Handles.Label(position, toDisplay, textStyle);
        }
    }
}