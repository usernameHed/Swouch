using swouch.extension.runtime.ui.animate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace swouch.extension.runtime.ui.animate.scale
{
    [ExecuteInEditMode]
    public class AnimateScale : AnimateBase
    {
        [SerializeField]
        private Vector3 _minScale = new Vector3(1, 1, 1);

        [SerializeField]
        private Vector3 _maxScale = new Vector3(2, 2, 2);
        [SerializeField]
        private Transform _toAnimate = default;

        protected override void Build()
        {
            base.Build();
            _isBuilded = true;
        }

        public override void Animate(UnityAction animationEnd = null, UnityAction animationCanceled = null)
        {
            if (!_isBuilded)
            {
                Build();
            }

            base.Animate(animationEnd, animationCanceled);
            DoAction();
        }

        public override void ResetAnimation(bool canCancel = false)
        {
            base.ResetAnimation();
            ResetAnimateAtEnd();
        }

        protected override void DoAction()
        {
            if (_toAnimate == null)
            {
                this.enabled = false;
                return;
            }
            _toAnimate.localScale = Vector3.LerpUnclamped(_minScale, _maxScale, GetEasedProgress());
        }

        public override void ResetAnimateAtEnd()
        {
            if (!_isBuilded)
            {
                return;
            }
            _toAnimate.localScale = _minScale;
        }

        public override void SetToFirstFrame()
        {
            _toAnimate.localScale = Vector3.LerpUnclamped(_minScale, _maxScale, Evaluate(0));
        }
    }
}