using swouch.extension.propertyAttribute.noNull;
using swouch.time;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace swouch.entity
{
    public class Jump : MonoBehaviour
    {
        [SerializeField] private float _force = 5;
        [SerializeField] private float _coolDownBetween2Jumps = 0.5f;

        //refs
        [SerializeField, NoNull] private Rigidbody _rigidBody = default;
        [SerializeField, NoNull] private PlayerInput _playerInput = default;
        [SerializeField, NoNull] private IsOnGround _isOnGround = default;

        public event UnityAction OnJump;

        private FrequencyCoolDown _coolDownTimerbetween2Jumps = new FrequencyCoolDown();

        public void CustomFixedUpdate()
        {
            if (CanJump())
            {
                Debug.Log("can jump!");
                JumpAction(Vector3.up, _force);
                _coolDownTimerbetween2Jumps.StartCoolDown(_coolDownBetween2Jumps);
            }
        }

        private bool CanJump()
        {
            return (_playerInput.InputJump && _isOnGround.IsGrounded && _coolDownTimerbetween2Jumps.IsNotRunning());
        }

        private void JumpAction(Vector3 direction, float speed)
        {
            _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, 0, _rigidBody.velocity.z);
            _rigidBody.AddForce(direction * speed, ForceMode.VelocityChange);
            OnJump?.Invoke();
        }
    }
}