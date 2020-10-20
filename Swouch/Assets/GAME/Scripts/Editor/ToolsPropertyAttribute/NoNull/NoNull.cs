using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.extension.propertyAttribute.noNull
{
    /// <summary>
    /// Cannot Be Null will red-flood the field if the reference is null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NoNull : PropertyAttribute
    {

    }
}