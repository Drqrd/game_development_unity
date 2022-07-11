using System.Collections.Generic;
using UnityEngine;

namespace Generation.Voronoi {
    public class Edge
    {
        public Vector3 A { get; private set; }
        public Vector3 B { get; private set; }

        public Vector3 Midpoint { get; private set; }
        public Edge(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;

            Midpoint = (A + B) / 2f;
        }

        public static void Log(Edge e)
        {
            #if UNITY_EDITOR
            Debug.Log($"Edge - A: {e.A}, B: {e.B}");
            #endif
        }
    }
}


