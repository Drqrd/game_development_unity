using UnityEngine;
using System.Collections.Generic;

namespace Generation.Voronoi 
{
    public class Triangle
    {
        public List<Triangle> Neighbors { get; set; }
        public Edge A { get; private set; }
        public Edge B { get; private set; }
        public Edge C { get; private set; }
        public int Index { get; private set; }
        public Vector3 Centroid { get; private set; }
        public Vector3[] Points { get; private set; }

        public Triangle(Vector3 a, Vector3 b, Vector3 c, int index)
        {
            Points = new Vector3[] { a, b, c };

            Neighbors = new List<Triangle>(3);

            A = new Edge(a, b);
            B = new Edge(b, c);
            C = new Edge(c, a);
            Index = index;

            Centroid = GetCentroid(a, b, c);
        }

        public static void Log(Triangle t)
        {
            #if UNITY_EDITOR
            Debug.Log($"- Triangle -");
            Debug.Log($"Centroid: {t.Centroid}");
            Edge.Log(t.A);
            Edge.Log(t.B);
            Edge.Log(t.C);
            Debug.Log($"------------");
            #endif
        }

        public static Vector3 GetCentroid(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return new Vector3((v1.x + v2.x + v3.x) / 3f, (v1.y + v2.y + v3.y) / 3f, (v1.z + v2.z + v3.z) / 3f);
        }
    }
}


