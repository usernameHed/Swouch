using System;
using UnityEngine;

namespace swouch.extension.propertyAttribute.onvalueChanged
{
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string MethodName;
        public OnValueChangedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}