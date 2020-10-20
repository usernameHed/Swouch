using swouch.time;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace swouch.extension.runtime.ui.animate
{
    [ExecuteInEditMode]
    public abstract class AnimateBase : MonoBehaviour
    {
        [SerializeField]
        private string _animationDescription = "";

        [SerializeField]
        protected float _timeAnimationBefore = 0f;
        [SerializeField]
        protected float _timeAnimation = 0.5f;
        [SerializeField]
        private AnimationCurve _ease = default;
        [SerializeField]
        protected bool _goBackToInitialPositionAtEnd = true;
        [SerializeField]
        private bool _loop = false;
        public void SetLoop(bool loop) { _loop = loop; }

        protected FrequencyChrono _before = new FrequencyChrono();
        protected FrequencyChrono _chrono = new FrequencyChrono();
        private UnityAction _animationEnd;
        private UnityAction _animationCanceled;
        public bool IsAnimating { get; private set; }
        protected bool _isBuilded = false;

        public abstract void SetToFirstFrame();
        protected abstract void DoAction();
        public abstract void ResetAnimateAtEnd();

        protected virtual void Build()
        {
            this.enabled = false;
            IsAnimating = false;
        }

        public virtual void ResetBuild()
        {
            _isBuilded = false;
        }

        public void SetTimeAnimation(float time)
        {
            _timeAnimation = time;
        }

        public void SetWaitTimeBeforeStart(float wait)
        {
            _timeAnimationBefore = wait;
        }

        public virtual void Animate()
        {
            Animate(null, null);
        }

        public virtual void Animate(UnityAction animationEnd = null, UnityAction animationCanceled = null)
        {
            if (!_isBuilded)
            {
                Build();
            }

            _animationEnd = animationEnd;
            _animationCanceled = animationCanceled;
            if (_chrono.IsRunning())
            {
                ResetAnimation(false);
            }

            if (_timeAnimationBefore > 0)
            {
                SetToFirstFrame();
                _before.StartChrono(_timeAnimationBefore, false);
            }
            else
            {
                _chrono.StartChrono(_timeAnimation, false);
            }

            this.enabled = true;
            IsAnimating = true;
        }

        public virtual void ResetAnimation(bool canCancel = false)
        {
            _before.Reset();
            _chrono.Reset();
            this.enabled = false;
            IsAnimating = false;
            if (canCancel)
            {
                _animationCanceled?.Invoke();
            }
        }

        protected virtual void Update()
        {
            CustomUpdate();
        }

        private void CustomUpdate()
        {
            if (_before.IsFinished())
            {
                _chrono.StartChrono(_timeAnimation, false);
            }
            else if (_before.IsRunning())
            {
                return;
            }
            
            if (_chrono.IsFinished(false))
            {
                DoAction();
                if (_loop)
                {
                    Animate(_animationEnd, _animationCanceled);
                    return;
                }
                else
                {
                    AnimateEnd();
                    _chrono.Reset();
                }
                return;
            }
            DoAction();
        }

        

        private void AnimateEnd()
        {
            if (_goBackToInitialPositionAtEnd || !Application.isPlaying)
            {
                ResetAnimateAtEnd();
            }
            

            this.enabled = false;
            IsAnimating = false;
            _animationEnd?.Invoke();
        }

        

        /// <summary>
        /// return progress, from 0 to 1
        /// </summary>
        /// <returns></returns>
        public float GetProgress()
        {
            return (_chrono.GetCurrentPercentFromTheEnd());
        }

        /// <summary>
        /// From a percent, from 0 to 1, set the timer
        /// </summary>
        /// <param name="percent"></param>
        public void AddProgressTime(float percent)
        {
            _chrono.MoveForwardOfXPercent(percent);
        }

        public float GetRealChrono()
        {
            return (_chrono.GetTimer());
        }

        public float GetRealInverseChrono()
        {
            return (_chrono.MaxTime - _chrono.GetTimer());
        }

        protected float GetEasedProgress()
        {
            return (_ease.Evaluate(_chrono.GetCurrentPercentFromTheEnd()));
        }

        protected float Evaluate(float percent)
        {
            return (_ease.Evaluate(percent));
        }
    }
}