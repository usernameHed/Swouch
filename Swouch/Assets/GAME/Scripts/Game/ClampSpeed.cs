using swouch.extension.propertyAttribute.noNull;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.entity
{
    public class ClampSpeed : MonoBehaviour
    {
        [SerializeField]
        private float _maxSpeedHoriz = 10f;
        [SerializeField]
        private float _maxSpeedVerti = 10f;

        [Tooltip("rigidbody"), SerializeField, NoNull]
        private Rigidbody _rb = null;

        private float _actualVelocity = 0f;
        public float GetActualVelocity() => _actualVelocity;

        public void CustomFixedUpdate()
        {
            _actualVelocity = _rb.velocity.magnitude;
            _rb.velocity = new Vector3(
                Mathf.Clamp(_rb.velocity.x, -_maxSpeedHoriz, _maxSpeedHoriz),
                 Mathf.Clamp(_rb.velocity.y, -_maxSpeedVerti, _maxSpeedVerti),
                 _rb.velocity.z);
        }
    }
}