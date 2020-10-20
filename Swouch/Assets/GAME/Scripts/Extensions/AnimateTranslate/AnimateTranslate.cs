using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace swouch.extension.runtime.ui.animate.translate
{
    [ExecuteInEditMode]
    public class AnimateTranslate : AnimateBase
    {
        [SerializeField]
        private Vector3 _finalTranslate = new Vector3(2, 2, 2);
        
        [SerializeField]
        private Transform _toAnimate = default;

        private Vector3 _initialLocalPosition = default;

        protected override void Build()
        {
            base.Build();
            _initialLocalPosition = _toAnimate.localPosition;
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
            _toAnimate.localPosition = _initialLocalPosition;
        }

        protected override void DoAction()
        {
            _toAnimate.localPosition = Vector3.LerpUnclamped(_initialLocalPosition, _finalTranslate, GetEasedProgress());
        }

        public override void ResetAnimateAtEnd()
        {
            if (!_isBuilded)
            {
                return;
            }
            _toAnimate.localPosition = _initialLocalPosition;
        }

        public override void SetToFirstFrame()
        {
            _toAnimate.localPosition = Vector3.LerpUnclamped(_initialLocalPosition, _finalTranslate, Evaluate(0));
        }
    }
}