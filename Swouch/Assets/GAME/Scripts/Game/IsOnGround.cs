using swouch.extension.propertyAttribute.noNull;
using swouch.time;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace swouch.entity
{
    public class IsOnGround : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }
        [SerializeField] private float _radiusCastSize = 2f;
        [SerializeField] private float _maxDistance = 2f;
        [SerializeField] private float _offsetMin = 0.1f;
        [SerializeField] private float _minTimeInAirWhenJump = 0.2f;

        [SerializeField, NoNull] private Rigidbody _rigidbody = default;
        [SerializeField, NoNull] private Jump _jump = default;

        private FrequencyCoolDown _coolDownInAirWhenJump = new FrequencyCoolDown();
        private int _layerMask = Physics.AllLayers;
        public event UnityAction OnGrounded;

        private void OnEnable()
        {
            _jump.OnJump += OnJump;
        }

        private void OnDisable()
        {
            _jump.OnJump -= OnJump;
        }

        public void CustomFixedUpdate()
        {
            if (_coolDownInAirWhenJump.IsNotRunning())
            {
                GroundCheck();
            }
        }

        public void GroundCheck()
        {
            Vector3 direction = Vector3.down;
            RaycastHit hitInfo;

            Vector3 dirRaycast = direction;
            float ditance = _radiusCastSize + _maxDistance;
            Vector3 position = _rigidbody.transform.position + Vector3.up * _offsetMin;

            Debug.DrawRay(position, dirRaycast * ditance, Color.blue);

            //Vector3 dirRaycast = playerGravity.GetMainAndOnlyGravity() * (radius + magnitudeToCheck);
            //Debug.DrawRay(rb.transform.position, dirRaycast * -1, Color.blue, 0.1f);
            if (Physics.SphereCast(position, _radiusCastSize, direction, out hitInfo,
                                   _maxDistance, _layerMask, QueryTriggerInteraction.Ignore))
            {
                if (!IsGrounded)
                {
                    OnGrounded?.Invoke();
                }
                IsGrounded = true;
            }
            else
            {
                IsGrounded = false;
            }
        }

        public void OnJump()
        {
            _coolDownInAirWhenJump.StartCoolDown(_minTimeInAirWhenJump);
            IsGrounded = false;
        }
    }
}