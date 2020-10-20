using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using unityEssentials.peek.extensions.editor;

namespace unityEssentials.peek.shaderOutline
{
    public class SnapshotSceneView
    {
        private const int MAX_SAVED_UI = 100;
        private const int SIZE_RENDER_TEXTURE = 100;
        private const int ACCURACY_MOUSE = 3;
        private Dictionary<GameObject, Texture2D> _snapShotsUI = new Dictionary<GameObject, Texture2D>(MAX_SAVED_UI);
        private SceneViewChangeEvent _sceneViewChangeEvent;
        private const string LAYER_VISIBLE_FOR_SNAPSHOT_CAMERA = "TransparentFX";
        private const string TMP_CAMERA = "SNAPSHOT - TMP CAMERA TO DESTROY";
        private const string TMP_CANVAS = "SNAPSHOT - TMP CANVAS TO DESTROY";
        private const string TMP_RENDER_COPY = "SNAPSHOT - TMP <Graphic> TO DESTROY";

        public void Init(SceneViewChangeEvent sceneViewChangeEvent)
        {
            _sceneViewChangeEvent = sceneViewChangeEvent;
            _sceneViewChangeEvent.CameraChange += CleanDictionary;

            Tools.visibleLayers = -1; // This lets us set which layers are visible in the scene view.
            SceneView.RepaintAll(); // We need to repaint the scene
        }

        public bool IsHoverTransparentPixel<T>(SceneView sceneView, T specialComponent) where T : Graphic
        {
            Texture2D snapShotData;

            if (!_snapShotsUI.TryGetValue(specialComponent.gameObject, out snapShotData))
            {
                snapShotData = CalculateSnapShot(sceneView, specialComponent);
            }
            if (snapShotData == null)
            {
                Debug.Log("texture null");
                return (false);
            }

            Vector2 mousePosition = new Vector2(
                Event.current.mousePosition.x / sceneView.position.width * snapShotData.width,
                snapShotData.height - (Event.current.mousePosition.y / (sceneView.position.height - 10) * snapShotData.height));

            //DebugShowSnapShotIntoRender(snapShotData, mousePosition, false);

            for (int i = (int)mousePosition.x - ACCURACY_MOUSE; i < mousePosition.x + ACCURACY_MOUSE; i++)
            {
                for (int k = (int)mousePosition.y - ACCURACY_MOUSE; k < mousePosition.y + ACCURACY_MOUSE; k++)
                {
                    Color colorPixel = snapShotData.GetPixel(i, k);
                    if (colorPixel != Color.clear)
                    {
                        return (true);
                    }
                }
            }
            return (false);
        }

        private static void DebugShowSnapShotIntoRender(Texture2D snapShotData, Vector2 mousePosition, bool showCursor)
        {
            if (showCursor)
            {
                for (int i = (int)mousePosition.x - ACCURACY_MOUSE; i < mousePosition.x + ACCURACY_MOUSE; i++)
                {
                    for (int k = (int)mousePosition.y - ACCURACY_MOUSE; k < mousePosition.y + ACCURACY_MOUSE; k++)
                    {
                        snapShotData.SetPixel(i, k, Color.red);
                    }
                }
                snapShotData.Apply();
            }

            GameObject canvas = GameObject.Find("RENDER CANVAS");
            if (canvas == null)
            {
                canvas = new GameObject("RENDER CANVAS", typeof(Canvas), typeof(CanvasScaler));
                //canvas.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
            }
            GameObject tmpRendu = GameObject.Find("RENDER");
            if (tmpRendu == null)
            {
                tmpRendu = new GameObject("RENDER");
                tmpRendu.AddComponent<RawImage>();
                //tmpRendu.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
            }
            tmpRendu.transform.SetParent(canvas.transform);
            RawImage rendu = tmpRendu.GetComponent<RawImage>();
            if (rendu == null)
            {
                rendu = tmpRendu.AddComponent<RawImage>();
            }
            rendu.texture = snapShotData;
            rendu.SetNativeSize();
        }

        private Texture2D CalculateSnapShot<T>(SceneView sceneView, T specialComponent) where T : Graphic
        {
            Camera mainCamera = sceneView.camera;

            //// setup camera
            CameraClearFlags saveClearFlags = mainCamera.clearFlags;
            Color saveBackGroundColor = mainCamera.backgroundColor;
            int saveCullingMask = Tools.visibleLayers;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.clear;

            //// setup canvas
            Canvas mainCanvas = specialComponent.GetComponentInParent<Canvas>();
            Camera saveCameraOfCanvas = mainCanvas.worldCamera;
            RenderMode saveRenderMode = mainCanvas.renderMode;
            mainCanvas.renderMode = RenderMode.WorldSpace;
            mainCanvas.worldCamera = mainCamera;

            //create additional canvas on the current component
            //to be able to set a layerMask only to that componnent
            Canvas specialCanvasToDeleteIfNecessary;
            bool hasCreatedNewCanvas = false;
            if (mainCanvas.gameObject.GetInstanceID() != specialComponent.gameObject.GetInstanceID())
            {
                specialCanvasToDeleteIfNecessary = specialComponent.gameObject.AddComponent<Canvas>();
                hasCreatedNewCanvas = true;
            }
            else
            {
                specialCanvasToDeleteIfNecessary = mainCanvas;
            }
            int saveLayerSpecialComponent = specialCanvasToDeleteIfNecessary.gameObject.layer;
            specialCanvasToDeleteIfNecessary.gameObject.layer = LayerMask.NameToLayer(LAYER_VISIBLE_FOR_SNAPSHOT_CAMERA);

            

            int width = (int)sceneView.position.width;
            int height = (int)sceneView.position.height;
            int newWidth;
            int newHeight;
            
            if (width < height)
            {
                newWidth = SIZE_RENDER_TEXTURE;
                newHeight = height * SIZE_RENDER_TEXTURE / width;
            }
            else
            {
                newHeight = SIZE_RENDER_TEXTURE;
                newWidth = width * SIZE_RENDER_TEXTURE / height;
            }

            //// setup tool layer visibility
            mainCamera.cullingMask = (1 << LayerMask.NameToLayer(LAYER_VISIBLE_FOR_SNAPSHOT_CAMERA));
            ////

            Texture2D snapShotData = TakeSnapshot(mainCamera, Color.clear, newWidth, newHeight);

            //// setup tool layer back
            mainCamera.cullingMask = saveCullingMask;
            ////

            //// setup camera back
            mainCamera.clearFlags = saveClearFlags;
            mainCamera.backgroundColor = saveBackGroundColor;
            ////

            //// setup canvas back
            mainCanvas.worldCamera = saveCameraOfCanvas;
            mainCanvas.renderMode = saveRenderMode;
            ////

            //// remove special canvas added if it was not the root canvas
            specialCanvasToDeleteIfNecessary.gameObject.layer = saveLayerSpecialComponent;
            if (hasCreatedNewCanvas)
            {
                GameObject.DestroyImmediate(specialCanvasToDeleteIfNecessary);
            }

            _snapShotsUI.Add(specialComponent.gameObject, snapShotData);
            return (snapShotData);
        }

        /// <summary>
        /// Takes a snapshot of whatever is in front of the camera and within the camera's culling mask and returns it as a Texture2D.
        /// </summary>
        /// <param name="backgroundColor">The background color to apply to the camera before taking the snapshot.</param>
        /// <param name="width">The width of the snapshot image.</param>
        /// <param name="height">The height of the snapshot image.</param>
        /// <returns>A Texture2D containing the captured snapshot.</returns>
        private Texture2D TakeSnapshot(Camera cam, Color backgroundColor, int width, int height)
        {
            // Set the background color of the camera
            cam.backgroundColor = backgroundColor;

            // Get a temporary render texture and render the camera
            cam.targetTexture = RenderTexture.GetTemporary(width, height, 24);
            cam.Render();

            // Activate the temporary render texture
            RenderTexture previouslyActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;

            // Extract the image into a new texture without mipmaps
            Texture2D texture = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
            texture.Apply(false);

            // Reactivate the previously active render texture
            RenderTexture.active = previouslyActiveRenderTexture;

            // Clean up after ourselves
            cam.targetTexture = null;
            RenderTexture.ReleaseTemporary(cam.targetTexture);

            // Return the texture
            return texture;
        }


        public void CleanDictionary()
        {
            //Debug.Log("dico cleared");
            _snapShotsUI.Clear();
        }

        #region static helper
        /// <summary>
        /// Saves a Texture2D as a PNG file.
        /// </summary>
        /// <param name="tex">The Texture2D to write to a file.</param>
        /// <param name="filename">The name of the file. This will be the current timestamp if not specified.</param>
        /// <param name="directory">The directory in which to save the file. This will be the game/Snapshots directory if not specified.</param>
        /// <returns>A FileInfo pointing to the created PNG file</returns>
        public static FileInfo SavePNG(Texture2D tex, string filename = "", string directory = "")
        {
            return SavePNG(tex.EncodeToPNG(), filename, directory);
        }

        /// <summary>
        /// Saves a byte array of PNG data as a PNG file.
        /// </summary>
        /// <param name="bytes">The PNG data to write to a file.</param>
        /// <param name="filename">The name of the file. This will be the current timestamp if not specified.</param>
        /// <param name="directory">The directory in which to save the file. This will be the game/Snapshots directory if not specified.</param>
        /// <returns>A FileInfo pointing to the created PNG file</returns>
        public static FileInfo SavePNG(byte[] bytes, string filename = "", string directory = "")
        {
            directory = directory != "" ? Directory.CreateDirectory(directory).FullName : Directory.CreateDirectory(Path.Combine(Application.dataPath, "../Snapshots")).FullName;
            filename = filename != "" ? SanitizeFilename(filename) + ".png" : System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff") + ".png";
            string filepath = Path.Combine(directory, filename);

            File.WriteAllBytes(filepath, bytes);

            return new FileInfo(filepath);
        }

        /// <summary>
        /// Sanitizes a filename string by replacing illegal characters with underscores.
        /// </summary>
        /// <param name="dirty">The unsanitized filename string.</param>
        /// <returns>A sanitized filename string with illegal characters replaced with underscores.</returns>
        private static string SanitizeFilename(string dirty)
        {
            string invalidFileNameChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidFileNameChars);

            return Regex.Replace(dirty, invalidRegStr, "_");
        }
        #endregion
    }
}