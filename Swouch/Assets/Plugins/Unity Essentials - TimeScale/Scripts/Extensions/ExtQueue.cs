using System.Collections.Generic;

namespace unityEssentials.timeScale.extensions
{
    public static class ExtQueue
    {
        public static float GetSum(this Queue<float> queue)
        {
            float sum = 0;
            foreach (float value in queue)
            {
                sum += value;
            }
            return (sum);
        }
    }
}