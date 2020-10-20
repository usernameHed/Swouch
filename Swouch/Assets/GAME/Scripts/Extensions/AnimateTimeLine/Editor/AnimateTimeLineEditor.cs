using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace swouch.extension.runtime.ui.animate.timeline
{
    [CustomEditor(typeof(AnimateTimeLine))]
    public class AnimateTimeLineEditor : Editor
    {
        protected AnimateTimeLine _animate;

        protected virtual void OnEnable()
        {
            _animate = (AnimateTimeLine)target;
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