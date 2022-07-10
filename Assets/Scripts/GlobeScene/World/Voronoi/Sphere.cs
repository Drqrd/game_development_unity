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
            GetVoronoiCells(vertices, triangles);
            BuildGameObjects(vertices, triangles);
        }


        // Constructs a flattened cube sphere mesh 
        private void GetVerticesAndTriangles(out Vector3[] vertices, out int[] triangles)
        {
            if (debugTimeTracker != null) debugTimeTracker.LogTimeStart("GetVerticesAndTriangles()");

            Dictionary<Vector3, List<int>> map = new Dictionary<Vector3, List<int>>();
            List<Vector3> vs = new List<Vector3>();
            List<int> ts = new List<int>();

            // Generate Vertices and Triangles
            // For each direction

            int index = 0, uniqueVIndex = 0;
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

                        vs.Add(pointOnSphere);

                        // If map already contains the vertex, that means that it is a border vertex, i to the map
                        if (!map.ContainsKey(pointOnSphere))
                        {
                            map.Add(pointOnSphere, new List<int>());
                            map[pointOnSphere].Add(uniqueVIndex);
                            uniqueVIndex++;
                        }

                        map[pointOnSphere].Add(index);

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

            // Collapse Vertices / Triangles
            // Track both unique vertices, and regular vertices.
            // Access map using regular vertices, get the unique vertex index associated with it
            for (int a = 0; a < ts.Count; a++)
            {
                ts[a] = map[vs[ts[a]]][0];
            }

            /*  SLOW
            int ind = 0;
            foreach(KeyValuePair<Vector3, List<int>> kvp in map)
            {
                // If there is more than one index per vertex, that means that the vertex is shared with another triangle i.e. border vertex.
                // We need to collapse the vertex (by replacing all the triangles with the first instance of i)

                for(int a = 0; a < kvp.Value.Count; a++)
                {
                    int[] indexes = Enumerable.Range(0, ts.Count).Where(i => ts[i] == kvp.Value[a]).ToArray();
                    for(int b = 0; b < indexes.Length; b++)
                    {
                        ts[indexes[b]] = ind;
                    }
                }
                ind++;
            }
            */

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Collapsed Vertices and Triangles");

            vertices = map.Keys.ToArray();
            triangles = ts.ToArray();

            if (debugTimeTracker != null) debugTimeTracker.LogTimeEnd();
        }

        private void GetVoronoiCells(Vector3[] vs, int[] ts)
        {
            // NEEDS IMPROVEMENT
            // Get all edges from int[] triangles (Vector3[2] { vertices[int], vertices[int] }) into a HashSet
            // for each vertex, query against HashSet to get neighbors
            // With all vertex neighbors, get all triangle neighbors for a triangle
            // For each cell, it is made up of the centroids of each of a vertex's neighboring triangles

            // Create dictionary by vertex?
            // We know that a vertex will have at least 2 triangles attached, and these two triangles will share 1 edge that connects to the vertex.

            if (debugTimeTracker != null) debugTimeTracker.LogTimeStart("GetVoronoiCells()");

            List<Triangle> triangles = new List<Triangle>();
            Dictionary<Edge, List<Triangle>> edgeDictionary = new Dictionary<Edge, List<Triangle>>(new Edge.CompareAsSegment());

            for (int a = 0; a < ts.Length; a += 3)
            {
                triangles.Add(new Triangle(vs[ts[a + 0]], vs[ts[a + 1]], vs[ts[a + 2]]));
            }

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Triangle Objects Generated");

            foreach(Triangle t in triangles)
            {
                if (!edgeDictionary.ContainsKey(t.A)) edgeDictionary.Add(t.A, new List<Triangle>());
                edgeDictionary[t.A].Add(t);
                if (!edgeDictionary.ContainsKey(t.B)) edgeDictionary.Add(t.B, new List<Triangle>());
                edgeDictionary[t.B].Add(t);
                if (!edgeDictionary.ContainsKey(t.C)) edgeDictionary.Add(t.C, new List<Triangle>());
                edgeDictionary[t.C].Add(t);
            }

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Edge Dictionary Generated");

            if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed("Triangle Neighbors Found");

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


