using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using IDebug;

namespace Generation.Voronoi 
{
    public class Sphere
    {
        private int resolution;
        private float jitter;
        private Vector3[] directions;
        private TimeTracker debugTimeTracker;

        public GameObject CubeSphereMesh { get; private set; }
        public GameObject VoronoiSphereMesh { get; private set; }

        public Sphere(int resolution, float jitter, bool logTime)
        {
            this.resolution = resolution;
            this.jitter = jitter;
            this.directions = new Vector3[6] { Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back };

            debugTimeTracker = (logTime) ? new TimeTracker() : null;

            Generate();
        }

        private void Generate()
        {
            Vector3[] vertices;
            int[] triangles;


            GetVerticesAndTriangles(out vertices, out triangles);
            // GetVoronoiCells(vertices, triangles);
            BuildGameObjects(vertices, triangles);
        }


        // Constructs a flattened cube sphere mesh
        // Assigns neighbors here using adjacency matrix
        private void GetVerticesAndTriangles(out Vector3[] vertices, out int[] triangles)
        {
            if (debugTimeTracker != null) debugTimeTracker.LogTimeStart("GetVerticesAndTriangles()");

            List<Vector3> vs = new List<Vector3>();
            List<int> ts = new List<int>();

            List<int>[] borderIndices = new List<int>[20];
            Dictionary<Vector3, int[]> borderIndicesLocalUp = new Dictionary<Vector3, int[]>();

            for (int a = 0; a < 20; a++) borderIndices[a] = new List<int>();

            borderIndicesLocalUp[Vector3.forward] = new int[] { 8, 9, 10, 11 };
            borderIndicesLocalUp[Vector3.up] = new int[] { 12, 13, 14, 8 };
            borderIndicesLocalUp[Vector3.down] = new int[] { 11, 15, 16, 17 };
            borderIndicesLocalUp[Vector3.left] = new int[] { 13, 18, 9, 15 };
            borderIndicesLocalUp[Vector3.right] = new int[] { 14, 10, 19, 16 };
            borderIndicesLocalUp[Vector3.back] = new int[] { 17, 19, 18, 12 };

            // Generate Vertices and Triangles
            // For each direction

            int index = 0;
            for (int f = 0; f < directions.Length; f++)
            {
                Vector3 localUp = directions[f];
                Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
                Vector3 axisB = Vector3.Cross(localUp, axisA);

                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        // Vertex generation
                        Vector2 percent = new Vector2(x, y) / (resolution - 1);
                        Vector3 pointOnCube = localUp + (percent.x - .5f) * 2f * axisA + (percent.y - .5f) * 2f * axisB;
                        Vector3 pointOnSphere = PointOnCubeToPointOnSphere(pointOnCube);

                        if (x == 0 || x == resolution - 1 || y == 0 || y == resolution - 1)
                        {
                            borderIndices[OrganizeBorderVertex(pointOnSphere, x, y, localUp, borderIndicesLocalUp)].Add(index);
                        }

                        vs.Add(pointOnSphere);

                        // Triangles
                        if (x != resolution - 1 && y != resolution - 1)
                        {
                            ts.Add(index);
                            ts.Add(index + resolution + 1);
                            ts.Add(index + resolution);

                            ts.Add(index);
                            ts.Add(index + 1);
                            ts.Add(index + resolution + 1);
                        }
                        index++;
                    }
                }
            }

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Generated Vertices and Triangles");


            HashSet<Vector3> vHash = new HashSet<Vector3>();
            foreach (Vector3 v in vs) vHash.Add(v);
            List<Vector3> collapsedVs = vHash.ToList();

            // Collapse Triangles
            foreach(List<int> vsToChange in borderIndices)
            {
                if (vsToChange.Count > 0)
                {
                    int vertIndex = collapsedVs.IndexOf(vs[vsToChange[0]]);
                    for (int vIndex = 0; vIndex < vsToChange.Count; vIndex++)
                    {
                        ts = ts.Select(t => t == vsToChange[vIndex] ? vertIndex : t).ToList();
                    }
                }
            }

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Collapsed Vertices and Triangles");

            vertices = collapsedVs.ToArray();
            triangles = ts.ToArray();

            Debug.Log(vertices.Length);
            Debug.Log(ts.IndexOf(Mathf.Max(triangles)));
            Debug.Log(triangles.Length);

            if (debugTimeTracker != null) debugTimeTracker.LogTimeEnd();
        }

        private void GetVoronoiCells(Vector3[] vs, int[] ts)
        {
            if (debugTimeTracker != null) debugTimeTracker.LogTimeStart("GetVoronoiCells()");

            // vertexIndex dictionary
            Dictionary<Edge, List<Triangle>> dict = new Dictionary<Edge, List<Triangle>>(new CompareAsSegment());
            List<Triangle> triangles = new List<Triangle>();

            for (int a = 0; a < ts.Length; a += 3)
            {
                Triangle triangle = new Triangle(vs[ts[a + 0]], vs[ts[a + 1]], vs[ts[a + 2]]);
                triangles.Add(triangle);

                Edge A = new Edge(vs[ts[a + 0]], vs[ts[a + 1]]);
                Edge B = new Edge(vs[ts[a + 1]], vs[ts[a + 2]]);
                Edge C = new Edge(vs[ts[a + 2]], vs[ts[a + 0]]);

                if (!dict.ContainsKey(A)) dict.Add(A, new List<Triangle>());
                dict[A].Add(triangle);
                if (!dict.ContainsKey(B)) dict.Add(B, new List<Triangle>());
                dict[B].Add(triangle);
                if (!dict.ContainsKey(C)) dict.Add(C, new List<Triangle>());
                dict[C].Add(triangle);
            }

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Triangle Objects Generated, Vertex Dictionary Generated");

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Triangle Neighbors Assigned");

            if (debugTimeTracker != null) debugTimeTracker.LogTimeEnd("GetVoronoiCells()");
        }

        private void BuildGameObjects(Vector3[] vertices, int[] triangles)
        {
            CubeSphereMesh = new GameObject("CubeSphereMesh");

            MeshFilter meshFilter = CubeSphereMesh.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh.vertices = vertices;
            meshFilter.sharedMesh.triangles = triangles;
            meshFilter.sharedMesh.RecalculateNormals();

            MeshRenderer meshRenderer = CubeSphereMesh.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Globe/Map");
        }

        private int OrganizeBorderVertex(Vector3 v, int x, int y, Vector3 localUp, Dictionary<Vector3,int[]> borderIndicesLocalUp)
        {
            // Corner vertices cover indices 0-7, dimensions seen below
            if (v == PointOnCubeToPointOnSphere(new Vector3(1, -1, 1))) return 0;
            else if (v == PointOnCubeToPointOnSphere(new Vector3(1, 1, 1))) return 1;
            else if (v == PointOnCubeToPointOnSphere(new Vector3(-1, -1, 1))) return 2;
            else if (v == PointOnCubeToPointOnSphere(new Vector3(-1, 1, 1))) return 3;
            else if (v == PointOnCubeToPointOnSphere(new Vector3(-1, 1, -1))) return 4;
            else if (v == PointOnCubeToPointOnSphere(new Vector3(1, 1, -1))) return 5;
            else if (v == PointOnCubeToPointOnSphere(new Vector3(1, -1, -1))) return 6;
            else if (v == PointOnCubeToPointOnSphere(new Vector3(-1, -1, -1))) return 7;

            // Edge vertices are as follows: 
            // - forward up     = 8
            // - forward left   = 9
            // - forward right  = 10
            // - forward down   = 11
            // - up up          = 12
            // - up left        = 13
            // - up right       = 14
            // - up down        = 8
            // - down up        = 11
            // - down left      = 15
            // - down right     = 16
            // - down down      = 17
            // - left up        = 13
            // - left left      = 18
            // - left right     = 9
            // - left down      = 15
            // - right up       = 14
            // - right left     = 10
            // - right right    = 19
            // - right down     = 16
            // - back up        = 17
            // - back left      = 19
            // - back right     = 18
            // - back down      = 12
            // Up
            else if (y == 0) return borderIndicesLocalUp[localUp][0];
            // Left
            else if (x == 0) return borderIndicesLocalUp[localUp][1];
            // Right
            else if (x == resolution - 1) return borderIndicesLocalUp[localUp][2];
            // Down
            else if (y == resolution - 1) return borderIndicesLocalUp[localUp][3];

            // Throw Error
            return -1;
        }

        public static Vector3 PointOnCubeToPointOnSphere(Vector3 p)
        {
            float x2 = p.x * p.x;
            float y2 = p.y * p.y;
            float z2 = p.z * p.z;

            float x = p.x * Mathf.Sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
            float y = p.y * Mathf.Sqrt(1 - (x2 + z2) / 2 + (x2 * z2) / 3);
            float z = p.z * Mathf.Sqrt(1 - (x2 + y2) / 2 + (x2 * y2) / 3);

            return new Vector3(x, y, z);
        }
    }
}


