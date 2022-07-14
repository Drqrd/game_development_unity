using System.Linq;
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
            List<Vector3> vList = new List<Vector3>();
            vList.Add(a);
            vList.Add(b);

            vList.Sort((a, b) => a.x > b.x ? 1 : a.x < b.x ? -1 : 
                                 a.y > b.y ? 1 : a.y < b.y ? -1 : 
                                 a.z > b.z ? 1 : a.z < b.z ? -1 : 0);

            A = vList[0];
            B = vList[1];

            Midpoint = (A + B) / 2f;

            vList.Clear();
        }

        public static void Log(Edge e)
        {
            #if UNITY_EDITOR
            Debug.Log($"Edge - A: {e.A}, B: {e.B}");
            #endif
        }
    }
}


