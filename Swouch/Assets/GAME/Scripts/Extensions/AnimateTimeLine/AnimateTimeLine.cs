using swouch.time;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace swouch.extension.runtime.ui.animate.timeline
{
    [ExecuteInEditMode]
    public class AnimateTimeLine : MonoBehaviour
    {
        [Serializable]
        public struct ActionsTimeLine
        {
            public string Name;
            public float Time;
            public UnityEvent UnityActions;
        }

        [SerializeField]
        private List<ActionsTimeLine> _actions = new List<ActionsTimeLine>(1);
        [SerializeField]
        private UnityEvent _closeActions = default;
        [SerializeField]
        private float _endAnimationTime = 5f;
        [SerializeField]
        private bool _resetAtEnd = false;
        [SerializeField]
        private bool _allowDestroyInEditor = false;

        private FrequencyChrono _timeLine = new FrequencyChrono();
        private bool _isAnimating = false;
        public bool IsAnimating { get { return (_isAnimating); } }
        private List<ActionsTimeLine> _currentActions = new List<ActionsTimeLine>();
        private UnityAction _endActionFunction;

        public void SetupTimeLine(UnityAction endAction = null)
        {
            _endActionFunction = endAction;
        }

        public void Animate()
        {            
            _timeLine.StartChrono(_endAnimationTime, false);
            _isAnimating = true;
            this.enabled = true;
            _currentActions.Clear();
            Append(_currentActions, _actions);
        }

        /// <summary>
        /// from a current list, append a second list at the end of the first list
        /// </summary>
        /// <typeparam name="T">type of content in the lists</typeparam>
        /// <param name="currentList">list where we append stuffs</param>
        /// <param name="listToAppends">list to append to the other</param>
        public static void Append<T>(IList<T> currentList, IList<T> listToAppends)
        {
            if (listToAppends == null)
            {
                return;
            }
            for (int i = 0; i < listToAppends.Count; i++)
            {
                currentList.Add(listToAppends[i]);
            }
        }

        private void Update()
        {
            if (!_isAnimating || _currentActions == null)
            {
                this.enabled = false;
                return;
            }

            for (int i = _currentActions.Count - 1; i >= 0; i--)
            {
                if (_currentActions[i].Time < _timeLine.GetTimer())
                {
                    _currentActions[i].UnityActions?.Invoke();
                    _currentActions.RemoveAt(i);
                }
            }

            if (_timeLine.IsFinished(false) && _resetAtEnd)
            {
                ResetAnimation();
                OnEndAction();
            }
        }

        public void ResetAnimation()
        {
            _timeLine.Reset();
            _isAnimating = false;
            if (this != null)
            {
                this.enabled = false;
            }
            _currentActions?.Clear();
            _closeActions?.Invoke();
        }

        private void OnEndAction()
        {
            _endActionFunction?.Invoke();
        }
    }
}