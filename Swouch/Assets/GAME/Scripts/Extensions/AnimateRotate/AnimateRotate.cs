using swouch.extension.propertyAttribute.noNull;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace swouch.extension.runtime.ui.animate.translate
{
    [ExecuteInEditMode]
    public class AnimateRotate : AnimateBase
    {
        [SerializeField]
        private Vector3 _finalRotate = new Vector3(0, 90, 0);

        [SerializeField, NoNull]
        private Transform _toAnimate = default;

        private Vector3 _initialLocalRotation;

        private Vector3 _finalLocalRotate;

        public override void ResetBuild()
        {
            base.ResetBuild();
            ResetAnimation();
        }

        protected override void Build()
        {
            base.Build();
            _initialLocalRotation = _toAnimate.localEulerAngles;
            _finalLocalRotate = _initialLocalRotation + _finalRotate;
            _isBuilded = true;
        }

        public void SetFinalRotate(Vector3 finalRotate)
        {
            _finalRotate = finalRotate;
        }

        public override void SetToFirstFrame()
        {
            ResetAnimation();
            _toAnimate.localEulerAngles = Vector3.LerpUnclamped(_initialLocalRotation, _finalLocalRotate, Evaluate(0));
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
            _toAnimate.localEulerAngles = Vector3.LerpUnclamped(_initialLocalRotation, _finalLocalRotate, GetEasedProgress());
        }

        public override void ResetAnimateAtEnd()
        {
            if (!_isBuilded)
            {
                return;
            }
            _toAnimate.localEulerAngles = _initialLocalRotation;
            _isBuilded = false;
        }

        
    }
}