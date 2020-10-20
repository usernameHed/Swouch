using swouch.extension.propertyAttribute.noNull;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.entity
{
    public class PlayerUpdater : MonoBehaviour
    {
        [SerializeField, NoNull] private PlayerInput _input = default;
        [SerializeField, NoNull] private IsOnGround _isOnGround = default;
        [SerializeField, NoNull] private Move _move = default;
        [SerializeField, NoNull] private Jump _jump = default;
        [SerializeField, NoNull] private Gravity _gravity = default;
        [SerializeField, NoNull] private ClampSpeed _clampSpeed = default;

        public void Update()
        {
            _input.CustomUpdate();
        }

        public void FixedUpdate()
        {
            _isOnGround.CustomFixedUpdate();
            _move.CustomFixedUpdate();
            _jump.CustomFixedUpdate();
            _gravity.CustomFixedUpdate();
            _clampSpeed.CustomFixedUpdate();
        }
    }
}