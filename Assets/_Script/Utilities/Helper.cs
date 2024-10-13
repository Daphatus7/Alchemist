using UnityEngine;

namespace _Script.Utilities
{
    public static class Helper
    {
        public static bool IsFaceLeft(float angle)
        {
            return angle is (>= 90 or <= 0) and (<= 270 or >= 360);
        }
    }
}