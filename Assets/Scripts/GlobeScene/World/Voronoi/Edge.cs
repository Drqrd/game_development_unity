using System.Collections.Generic;
using UnityEngine;

namespace Generation.Voronoi {
    public class Edge
    {
        public Vector3 A { get; private set; }
        public Vector3 B { get; private set; }

        public Edge(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;
        }

        public static void Log(Edge e)
        {
            #if UNITY_EDITOR
            Debug.Log($"Edge - A: {e.A}, B: {e.B}");
            #endif
        }

        public sealed class CompareAsSegment : IEqualityComparer<Edge>
        {
            public bool Equals(Edge edgeOne, Edge edgeTwo)
            {
                return (edgeOne.A == edgeTwo.A && edgeOne.B == edgeTwo.B) ||
                       (edgeOne.A == edgeTwo.B && edgeOne.B == edgeTwo.A);
            }

            public int GetHashCode(Edge edge)
            {
                return (edge.A.x.GetHashCode() ^ edge.A.y.GetHashCode() << 2 ^ edge.A.z.GetHashCode() >> 2) ^
                       (edge.B.x.GetHashCode() ^ edge.B.y.GetHashCode() << 2 ^ edge.B.z.GetHashCode() >> 2);
            }
        }
    }
}


