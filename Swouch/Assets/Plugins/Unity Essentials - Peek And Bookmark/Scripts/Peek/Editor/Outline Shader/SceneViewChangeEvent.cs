using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using unityEssentials.peek.extensions.editor;

namespace unityEssentials.peek.shaderOutline
{
    public class SceneViewChangeEvent
    {
        public event UnityAction CameraChange;

        private const float COOL_DOWN_TIME = 1f;
        private Vector2 _dimensionChange;
        private Vector3 _positionCamera;
        private Quaternion _rotationCamera;
        private float _zoomCamera;
        private EditorChrono _coolDownEvents = new EditorChrono();

        public void Init()
        {
#if UNITY_2018_4_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif

            if (SceneView.lastActiveSceneView == null)
            {
                return;
            }
            _dimensionChange = new Vector2(SceneView.lastActiveSceneView.position.width, SceneView.lastActiveSceneView.position.height);
            _positionCamera = SceneView.lastActiveSceneView.camera.transform.position;
            _rotationCamera = SceneView.lastActiveSceneView.camera.transform.rotation;
            _zoomCamera = Vector3.SqrMagnitude(SceneView.lastActiveSceneView.pivot - _positionCamera);
            _coolDownEvents.StartChrono(COOL_DOWN_TIME);
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (_coolDownEvents.IsRunning())
            {
                return;
            }

            Vector2 dimension = new Vector2(SceneView.lastActiveSceneView.position.width, SceneView.lastActiveSceneView.position.height);
            Vector3 positionCamera = SceneView.lastActiveSceneView.camera.transform.position;
            Quaternion rotationCamera = SceneView.lastActiveSceneView.camera.transform.rotation;
            float zoomCamera = Vector3.SqrMagnitude(SceneView.lastActiveSceneView.pivot - _positionCamera);
            if (!EqualApprox(_dimensionChange, dimension)
                || !EqualApprox(_positionCamera, positionCamera)
                || !EqualApprox(_rotationCamera, rotationCamera)
                || !EqualApprox(_zoomCamera, zoomCamera))
            {
                _dimensionChange = dimension;
                _positionCamera = positionCamera;
                _rotationCamera = rotationCamera;
                _zoomCamera = zoomCamera;
                //Debug.Log("something change ??");
                CameraChange?.Invoke();
                _coolDownEvents.StartChrono(COOL_DOWN_TIME);
            }
        }

        public bool EqualApprox(float a, float b, float offset = 0.0001f)
        {
            float diff = a - b;
            return (diff >= 0 && diff < offset || diff < 0 && diff > -offset);
        }
        public bool EqualApprox(Vector3 a, Vector3 b, float offset = 0.0001f)
        {
            float diff = (a.x + a.y + a.z) - (b.x + b.y + b.z);
            return (diff >= 0 && diff < offset || diff < 0 && diff > -offset);
        }
        public bool EqualApprox(Vector2 a, Vector2 b, float offset = 0.0001f)
        {
            float diff = (a.x + a.y) - (b.x + b.y);
            return (diff >= 0 && diff < offset || diff < 0 && diff > -offset);
        }
        public bool EqualApprox(Quaternion a, Quaternion b, float offset = 0.0001f)
        {
            float diff = (a.x + a.y + a.z + a.w) - (b.x + b.y + b.z + b.w);
            return (diff >= 0 && diff < offset || diff < 0 && diff > -offset);
        }
    }
}