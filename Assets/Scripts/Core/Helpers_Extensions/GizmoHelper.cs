using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Assertions;

namespace GrappleGame.GizmoHelper
{
    public class GizmoHelper : MonoBehaviour
    {
        /// <summary>
        /// Points array should created in this order: { TOP LEFT, TOP RIGHT, BOTTOM RIGHT, BOTTOM LEFT } (vertices)
        /// </summary>
        /// <param name="points"></param>
        /// <exception cref="Exception"></exception>
        public static void DrawBox(Vector3[] points)
        {
            if (points.Length != 4) { throw new Exception("DrawBox requires exactly four points that correspond to the four vertices of a rectangle");  }

            Vector3[] lineList = new Vector3[] { points[0], points[1], points[1], points[2], points[2], points[3], points[3], points[0] };

            Gizmos.DrawLineList(new ReadOnlySpan<Vector3>(lineList));
        }
    }
}
