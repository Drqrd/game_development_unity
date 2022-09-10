using UnityEngine;

namespace Generation.Voronoi
{
    public class Cell
    {
        public Vector3 Center { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public Vector3[] TVertices { get; private set; }
        public int[] Triangles { get; private set; }
        public Cell Neighbors { get; set; }

        public Cell(Vector3[] vertices)
        {
            Vertices = vertices;

            Center = GetCenter();
            TVertices = GetTVertices();
            Triangles = Triangulate();
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

        private Vector3[] GetTVertices() {
            Vector3[] tVertices = new Vector3[Vertices.Length + 1];
            tVertices[0] = Center;

            for(int a = 0; a < Vertices.Length; a++) {
                tVertices[a + 1] = Vertices[a];
            }

            return tVertices;
        }

        private int[] Triangulate()
        {
            int[] triangles = new int[Vertices.Length * 3];
            Vector3 cross;
            float sign;


            for(int a = 0; a < Vertices.Length - 1; a++) {
                cross = Vector3.Cross(Vertices[a] - Center, Vertices[a + 1] - Center);
                sign = Mathf.Sign(Vector3.Dot(cross, Center));

                triangles[a * 3 + 0] = 0;
                triangles[a * 3 + 1] = sign > 0 ? a + 1 : a + 2;
                triangles[a * 3 + 2] = sign > 0 ? a + 2 : a + 1;
            }
            
            cross = Vector3.Cross(Vertices[Vertices.Length - 1] - Center, Vertices[0] - Center);
            sign = Mathf.Sign(Vector3.Dot(cross, Center));

            triangles[triangles.Length - 3] = 0;
            triangles[triangles.Length - 2] = sign > 0 ? Vertices.Length : 1;
            triangles[triangles.Length - 1] = sign > 0 ? 1 : Vertices.Length;
            return triangles;
        }
    }
}

