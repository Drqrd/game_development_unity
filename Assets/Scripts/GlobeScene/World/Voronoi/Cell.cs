using UnityEngine;

namespace Generation.Voronoi
{
    public class Cell
    {
        public Vector3 Center { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public int[] Triangles { get; private set; }
        public Cell Neighbors { get; set; }

        public Cell(Vector3[] vertices)
        {
            Vertices = vertices;
            Center = GetCenter();
            Triangulate();
        }

        private Vector3 GetCenter()
        {
            Vector3 c = Vector3.zero;
            foreach(Vector3 vertex in Vertices)
            {
                c += vertex;
            }
            c /= Vertices.Length;

            return c;
        }

        private int[] Triangulate()
        {
            return new int[] { };
        }
    }
}

