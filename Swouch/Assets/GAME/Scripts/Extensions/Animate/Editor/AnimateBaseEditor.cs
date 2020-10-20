using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace swouch.extension.runtime.ui.animate
{
    [CustomEditor(typeof(AnimateBase))]
    public abstract class AnimateBaseEditor : Editor
    {
        protected AnimateBase _animate;

        protected virtual void OnEnable()
        {
            _animate = (AnimateBase)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            OnCustomInspectorGUI();
        }

        private void OnCustomInspectorGUI()
        {
            using (HorizontalScope horizontalScope = new HorizontalScope())
            {
                if (GUILayout.Button("Play"))
                {
                    _animate.ResetAnimation();
                    _animate.Animate();
                }
                if (GUILayout.Button("Reset"))
                {
                    _animate.ResetAnimation();
                }
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying && _animate && _animate.IsAnimating)
            {
                _animate.ResetAnimation();
            }
        }
    }
}