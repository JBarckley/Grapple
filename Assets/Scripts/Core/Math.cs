using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GrappleGame.Math
{
    public static class Math
    {
        public static bool Near(Vector2 first, Vector2 second, float range)
        {
            return Mathf.Abs(first.x - second.x) < range && Mathf.Abs(first.y - second.y) < range;
        }

        public static float Sign(this Vector2 v)
        {
            if (Mathf.Sign(v.x) == Mathf.Sign(v.y)) { return Mathf.Sign(v.x); }
            else { return 1; }
        }

        public static Vector2 Rotate(this Vector2 vec, float angle)
        {
            Vector2 a = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 b = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));

            return (vec.x * a) + (vec.y * b);
        }

        /// <summary>
        /// part-wise division
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector3 Divide(this Vector3 left, Vector3 right)
        {
            return new Vector2(left.x / right.x, left.y / right.y);
        }
    }
}
