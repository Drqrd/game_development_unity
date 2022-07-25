using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using IDebug;

using IMathf = IHelper.Mathf;

namespace Generation.Voronoi 
{
    public class Sphere : BaseObject
    {
        private int resolution;
        private float jitter;
        private Vector3[] directions;
        private DebugColors debugColors;
        private DebugProperties debugProperties;

        public GameObject CubeSphereMesh { get; private set; }
        public GameObject VoronoiSphereMesh { get; private set; }
        public Cell VoronoiCells { get; private set; }

        

        public Sphere(int resolution, float jitter, DebugProperties debugProperties)
        {
            this.resolution = resolution;
            this.jitter = jitter;
            this.debugProperties = debugProperties;

            this.directions = new Vector3[6] { Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back };

            Generate();
        }

        private void Generate()
        {
            BuildDebug();

            Vector3[] vertices;
            int[] triangles;
            Color[] colors;

            GetVerticesAndTriangles(out vertices, out triangles, out colors);
            GetVoronoiCells(vertices, triangles);
            BuildGameObjects(vertices, triangles, colors);
        }


        // Constructs a flattened cube sphere mesh
        // Assigns neighbors here using adjacency matrix
        private void GetVerticesAndTriangles(out Vector3[] vertices, out int[] triangles, out Color[] colors)
        {
            TryLogStart("GetVerticesAndTriangles()");

            Dictionary<Vector3,int> vDict = new Dictionary<Vector3,int>();
            List<Vector3> vs = new List<Vector3>();
            List<int> ts = new List<int>();
            List<Color> cs = new List<Color>();
            List<Triangle> triangleObjs = new List<Triangle>();
            // Generate Vertices and Triangles
            // For each direction

            int index = 0, vIndex = 0;
            for (int f = 0; f < directions.Length; f++)
            {
                Vector3 localUp = directions[f];
                
                Vector3 axisA;
                if (localUp == Vector3.forward) axisA = Vector3.left;
                else if (localUp == Vector3.up) axisA = Vector3.left;
                else if (localUp == Vector3.down) axisA = Vector3.left;
                else if (localUp == Vector3.left) axisA = Vector3.back;
                else if (localUp == Vector3.right) axisA = Vector3.forward;
                else axisA = Vector3.right;

                Vector3 axisB = Vector3.Cross(localUp,axisA);

                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        // Vertex generation
                        Vector2 percent = new Vector2(x, y) / (resolution - 1);
                        Vector3 pointOnCube = localUp + (percent.x - .5f) * 2f * axisA + (percent.y - .5f) * 2f * axisB;
                        Vector3 pointOnSphere = pointOnCube; // PointOnCubeToPointOnSphere(pointOnCube);

                        vs.Add(pointOnSphere);
                        

                        if (!vDict.ContainsKey(pointOnSphere)) {
                            vDict.Add(pointOnSphere, vIndex++);
                        }

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
            TryLogElapsed("Generated Vertices and Triangles");

            // Collapse Triangles
            if (!debugProperties.uniqueTriangles)
            {
                for (int a = 0; a < ts.Count; a++)
                {
                    ts[a] = vDict[vs[ts[a]]];
                }
            }

            List<Vector3> tvs = new List<Vector3>(); ;
            if (debugProperties.uniqueTriangles)
            {
                for (int a = 0; a < ts.Count; a++)
                {
                    tvs.Add(vs[ts[a]]);
                    ts[a] = a;
                }
            }

            TryLogElapsed("Collapsed Vertices and Triangles");
            
            for (int a = 0; a < ts.Count; a += 3)
            {
                if (debugProperties.uniqueTriangles) triangleObjs.Add(new Triangle(tvs[ts[a + 0]], tvs[ts[a + 1]], tvs[ts[a + 2]]));
                else triangleObjs.Add(new Triangle(vs[ts[a + 0]], vs[ts[a + 1]], vs[ts[a + 2]]));

            }
            TryLogElapsed("Generated Triangle Objects");

            triangleObjs = FindTriangleNeighbors(triangleObjs, out cs);

            TryLogElapsed("Assigned Triangle Neighbors");

            vertices = debugProperties.uniqueTriangles ? tvs.ToArray() : vDict.Keys.ToArray();
            triangles = ts.ToArray();
            colors = cs.ToArray();
            TryLogEnd();

            // LOCAL FUNCTIONS
            List<Triangle> FindTriangleNeighbors(List<Triangle> ts, out List<Color> cs)
            {
                cs = new List<Color>();

                int[][] faceNeighbors = new int[6][] {
                new int[4] { 1, 3, 4, 2 },
                new int[4] { 5, 3, 4, 0 },
                new int[4] { 0, 3, 4, 5 },
                new int[4] { 1, 5, 0, 2 },
                new int[4] { 1, 0, 5, 2 },
                new int[4] { 2, 3, 4, 1 }};

                int trianglesPerFace = (resolution - 1) * (resolution - 1) * 2;
                int trianglesAcross = (resolution - 1) * 2;
                int lastRow = trianglesPerFace - trianglesAcross;

                for (int a = 0; a < ts.Count; a++)
                {
                    int triIndex = a % trianglesPerFace;
                    int currentFace = a / trianglesPerFace;

                    // TopRightCorner Triangle -- DEBUG COLOR: Red
                    if (triIndex == trianglesAcross - 1)
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[0]);
                        
                        // Top Neighbor
                        // Interior Neighbor
                        // Left Neighbor
                    }
                    // BottomLeftCornerTriangle -- DEBUG COLOR: Blue
                    else if (triIndex == trianglesPerFace - trianglesAcross)
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[1]);
                        
                        // Interior Neighbor
                        // Right Neighbor
                        // Bottom Neighbor
                    }
                    // Top Triangle -- DEBUG COLOR: Yellow
                    else if (triIndex < trianglesAcross && IMathf.IsEven(triIndex))
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[2]);

                        // Top Neighbor
                        // Interior Right Neighbor
                        // Interior Left Neighbor
                    }
                    // Left Triangle -- DEBUG COLOR: Cyan
                    else if (triIndex % trianglesAcross == 0)
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[3]);
                    }
                    // Right Triangle -- DEBUG COLOR: Magenta
                    else if (triIndex % trianglesAcross == trianglesAcross - 1)
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[4]);

                    }
                    // Bottom Triangle -- DEBUG COLOR: Green
                    else if (triIndex < lastRow && IMathf.IsOdd(triIndex))
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[5]);

                    }
                    // Even Interior Triangle -- DEBUG COLOR: White
                    else if (IMathf.IsEven(triIndex))
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[6]);

                    }
                    // Odd Interior Triangle -- DEBUG COLOR: Gray
                    else
                    {
                        // Self
                        if (debugProperties.uniqueTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[6]);

                    }
                }
                return ts;
            }
        }

        private void GetVoronoiCells(Vector3[] vs, int[] ts)
        {
            TryLogStart("GetVoronoiCells()");

            TryLogElapsed("Triangle Objects Generated");

            TryLogElapsed("Triangle Neighbors Assigned");

            TryLogElapsed("Voronoi Cell Objects Generated");

            TryLogEnd();
        }

        private void BuildGameObjects(Vector3[] vertices, int[] triangles, Color[] colors)
        {
            CubeSphereMesh = new GameObject("CubeSphereMesh");

            MeshFilter meshFilter = CubeSphereMesh.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;

            meshFilter.sharedMesh = mesh;
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

        /* ---------- DEBUG ---------- */

        public struct DebugColors
        {
            /*
            public Color[] VertexColorArray { get; private set; }
            public Color[] TriangleColorArray { get; private set; }
            public Color[] FaceColorArray { get; private set; }
            */

            public Color[] VertexColorSet { get; private set; }
            public Color[] TriangleColorSet { get; private set; }
            public Color[] FaceColorSet { get; private set; }

            public DebugColors(Color[] vcs, Color[] tcs, Color[] fcs) // Color[] vca, Color[] tca, Color[] fca)
            {
                VertexColorSet = vcs;
                TriangleColorSet = tcs;
                FaceColorSet = fcs;
            }
        }

        public struct DebugProperties
        {
            public bool uniqueTriangles { get; private set; }
            public bool logTime { get; private set; }

            public DebugProperties(bool ut, bool lt)
            {
                uniqueTriangles = ut;
                logTime = lt;
            }
        }

        private void BuildDebug()
        {
            debugTimeTracker = (debugProperties.logTime) ? new TimeTracker() : null;

            // Forward, Up, Down, Left, Right, Back
            Color[] faceColorSet = new Color[6] { Color.white, Color.blue, Color.red, Color.yellow, Color.green, Color.gray };

            // Start, End, Top, Left, Right, Down, InteriorEven, InteriorOdd
            Color[] triangleColorSet = new Color[8] { Color.red, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.green, Color.white, Color.gray };

            // CornerTopLeft, CornerTopRight, CornerBottomLeft, CornerBottomRight, Top, Left, Right, Bottom, Interior
            Color[] vertexColorSet = new Color[9] { Color.red, Color.blue, Color.yellow, Color.green, Color.magenta, Color.cyan, new Color(1, .4f, 0), Color.black, Color.white };

            debugColors = new DebugColors(vertexColorSet, triangleColorSet, faceColorSet);
        }
    }
}


