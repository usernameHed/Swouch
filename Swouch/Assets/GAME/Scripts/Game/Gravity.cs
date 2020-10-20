using swouch.extension.propertyAttribute.noNull;
using swouch.time;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.entity
{
    public class Gravity : MonoBehaviour
    {
        [SerializeField] private float _gravityOnGround = 9.81f;
        [SerializeField] private float _gravityInAir = 6f;
        [SerializeField] private float _gravityInAirDown = 17f;
        [SerializeField] private float _timeBeforeItsOkToGoDown = 0.2f;

        //refs
        [SerializeField, NoNull] private Rigidbody _rigidBody = default;
        [SerializeField, NoNull] private IsOnGround _isOnGround = default;
        [SerializeField, NoNull] private PlayerInput _input = default;
        [SerializeField, NoNull] private Jump _jump = default;


        private FrequencyCoolDown _timerbeforeItsOkToGoDown = new FrequencyCoolDown();
        private bool _isGoingDown = false;
        private bool _wantToGoDown = false;

        private void OnEnable()
        {
            _jump.OnJump += OnJump;
            _isOnGround.OnGrounded += OnGround;
        }

        private void OnDisable()
        {
            _jump.OnJump -= OnJump;
            _isOnGround.OnGrounded -= OnGround;
        }

        public void CustomFixedUpdate()
        {
            Vector3 finalVelocity;
            if (_isOnGround.IsGrounded)
            {
                finalVelocity = Vector3.down * _gravityOnGround * Time.fixedDeltaTime * 100;
            }
            else
            {
                if (ShouldApplyGravityDown())
                {
                    finalVelocity = Vector3.down * _gravityInAirDown * Time.fixedDeltaTime * 100;
                }
                else
                {
                    finalVelocity = Vector3.down * _gravityInAir * Time.fixedDeltaTime * 100;
                }
            }
            MoveAction(finalVelocity);
        }

        private bool ShouldApplyGravityDown()
        {
            _wantToGoDown = !_input.InputJump;
            if (_wantToGoDown && _timerbeforeItsOkToGoDown.IsNotRunning())
            {
                return (true);
            }

            float dotGravityRigidbody = Vector3.Dot(Vector3.down, _rigidBody.velocity);
            if (dotGravityRigidbody < 0)
            {
                _isGoingDown = false;
            }
            else
            {
                _isGoingDown = true;
            }
            return (_isGoingDown);
        }

        public void OnJump()
        {
            _timerbeforeItsOkToGoDown.StartCoolDown(_timeBeforeItsOkToGoDown);
        }

        public void OnGround()
        {
            _timerbeforeItsOkToGoDown.Reset();
        }

        private void MoveAction(Vector3 direction)
        {
            _rigidBody.AddForce(direction, ForceMode.Acceleration);
        }
    }
}