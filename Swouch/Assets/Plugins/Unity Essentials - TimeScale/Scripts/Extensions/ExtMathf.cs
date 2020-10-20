using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace unityEssentials.timeScale.extensions
{
    public static class ExtMathf
    {
        /// <summary>
        /// number convert range (55 from 0 to 100, to a base 0 - 1 for exemple)
        /// </summary>
        public static double Remap(this double value, double from1, double to1, double from2, double to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}