using swouch.extension.propertyAttribute.noNull;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.entity
{
    public class Move : MonoBehaviour
    {
        [SerializeField] private float _speed = 5;
        [SerializeField] private float _speedInverseMultiplier = 1;
        [SerializeField] private float _slowDownSpeed = 5;
        [SerializeField] private float _maxSpeed = 20;

        //refs
        [SerializeField, NoNull] private Rigidbody _rigidBody = default;
        [SerializeField, NoNull] private PlayerInput _input = default;

        public void CustomFixedUpdate()
        {
            if (_input.IsMovingLaterally)
            {
                Vector3 direction = new Vector3(_input.InputHoriz, 0, 0);
                float multiplierInverse = 1;
                bool isInputAndInertiaInverted = (_rigidBody.velocity.x < 0 && _input.InputHoriz > 0)
                                            || (_rigidBody.velocity.x > 0 && _input.InputHoriz < 0);
                if (isInputAndInertiaInverted)
                {
                    multiplierInverse = _speedInverseMultiplier;
                }
                MoveAction(direction, _speed * multiplierInverse);
            }
            else
            {
                Vector3 inverseDirection = new Vector3(-_rigidBody.velocity.x, 0, 0);
                MoveAction(inverseDirection, _slowDownSpeed);
            }
        }

        private void MoveAction(Vector3 direction, float speed)
        {
            _rigidBody.AddForce(direction * Time.fixedDeltaTime * speed, ForceMode.VelocityChange);
        }
    }
}