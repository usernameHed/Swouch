using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.entity
{
    public class PlayerInput : MonoBehaviour
    {
        public float InputHoriz { get; private set; }
        public float InputVerti { get; private set; }
        public bool InputJump { get; private set; }

        public bool IsMovingLaterally { get; private set; }

        public void CustomUpdate()
        {
            InputHoriz = Input.GetAxis("Horizontal");
            InputVerti = Input.GetAxis("Vertical");
            IsMovingLaterally = InputHoriz != 0;
            InputJump = Input.GetButton("Jump");
        }
    }
}