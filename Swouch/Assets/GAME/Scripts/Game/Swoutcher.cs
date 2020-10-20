using swouch.extension.runtime.ui.animate.timeline;
using swouch.time;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.entity
{
    public class Swoutcher : MonoBehaviour
    {
        [SerializeField] private float _waitTime = 1f;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _firstKick = 1f;
        [SerializeField] private AnimateTimeLine _animateTimeLine = default;
        [SerializeField] private Rigidbody _rigidbody = default;
        [SerializeField] private Transform _target;

        private FrequencyCoolDown _waitBeforeAttack = new FrequencyCoolDown();

        public void Build(Transform target)
        {
            _target = target;
            Attack();
        }

        public void Attack()
        {
            _rigidbody.transform.LookAt(_target.transform);
            _waitBeforeAttack.StartCoolDown(_waitTime);
            _animateTimeLine.Animate();
        }

        private void FixedUpdate()
        {
            if (_waitBeforeAttack.IsFinished())
            {
                _rigidbody.AddForce(_rigidbody.transform.forward * _firstKick, ForceMode.Impulse);
            }
            if (_waitBeforeAttack.IsRunning())
            {
                return;
            }
            _rigidbody.AddForce(_rigidbody.transform.forward * _speed * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }
}