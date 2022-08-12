using UnityEngine;
using System;
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

        public Edge[] testEdges { get; private set; }
        public Cell[] testCells { get; private set; }
        public Triangle[] tObjs { get; private set; }

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

            Vector3[] vertices = new Vector3[] { };
            int[] triangles = new int[] { };
            Triangle[] triangleObjs;
            Color[] colors;

            GetVerticesAndTriangles(out triangleObjs, out vertices, out colors);
            GetVoronoiCells(triangleObjs, vertices);
            BuildGameObjects(vertices, triangles, triangleObjs, colors);
        }


        // Constructs a flattened cube sphere mesh
        // Assigns neighbors here using adjacency matrix
        private void GetVerticesAndTriangles(out Triangle[] triangleObjs, out Vector3[] vertices, out Color[] colors)
        {
            TryLogStart("GetVerticesAndTriangles()");

            Dictionary<Vector3,int> vDict = new Dictionary<Vector3,int>();
            List<Vector3> vs = new List<Vector3>();
            List<int> ts = new List<int>();
            List<Color> cs = new List<Color>();
            List<Triangle> tObjs = new List<Triangle>();
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
                        Vector3 pointOnSphere = debugProperties.disableSphereTransformation ? pointOnCube : PointOnCubeToPointOnSphere(pointOnCube);

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

            List<Vector3> tvs = new List<Vector3>();
            for (int a = 0; a < ts.Count; a++)
            {
                tvs.Add(vs[ts[a]]);
                ts[a] = a;
            }
            TryLogElapsed("Collapsed Vertices and Triangles");
            
            for (int a = 0; a < ts.Count; a += 3) tObjs.Add(new Triangle(tvs[ts[a + 0]], tvs[ts[a + 1]], tvs[ts[a + 2]], a / 3));

            TryLogElapsed("Generated Triangle Objects");

            tObjs = FindTriangleNeighbors(tObjs, out cs);

            TryLogElapsed("Assigned Triangle Neighbors");

            triangleObjs = tObjs.ToArray();
            colors = cs.ToArray();
            vertices = vs.ToArray();

            TryLogEnd();

            // LOCAL FUNCTIONS
            List<Triangle> FindTriangleNeighbors(List<Triangle> ts, out List<Color> cs)
            {
                cs = new List<Color>();

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
                        // Self - Debug Coloring
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[0]);

                        // Forward
                        if (currentFace == 0)
                        {
                            // Top Neighbor - Last Triangle, Last Row, Top Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 - 2]);
                            // Right Neighbor - First Triangle, First Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3]);
                            // Interior Neighbor - Last Triangle - 1, First Row, Forward Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Up
                        else if (currentFace == 1)
                        {
                            // Top Neighbor - 2nd Triangle, First Row, Back Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 5 + 1]);
                            // Right Neighbor - Last Triangle, First Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 + trianglesAcross - 1]);
                            // Interior Neighbor - Last Triangle - 1, First Row, Top Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Down
                        else if (currentFace == 2)
                        {
                            // Top Neighbor - Last Triangle - 1, Last Row, Front Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 1 - 2]);
                            // Right Neighbor - First Triangle, Last Row, Left Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 - trianglesAcross]);
                            // Interior Neighbor - Last Triangle - 1, First Row, Down Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Left
                        else if (currentFace == 3)
                        {
                            // Top Neighbor - Last Triangle, First Row, Top Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 1 + trianglesAcross - 1]);
                            // Right Neighbor - First Triangle, First Row, Back Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 5]);
                            // Interior Neighbor - Last Triangle - 1, First Row, Left Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                        
                        
                        // Right
                        else if (currentFace == 4)
                        {
                            // Top Neighbor - First Triangle, Last Row, Top Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 - trianglesAcross]);
                            // Right Neighbor - First Triangle, First Row, Front Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 0]);
                            // Interior Neighbor - Last Triangle - 1, First Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                        
                        // Back
                        else if (currentFace == 5)
                        {
                            // Top Neighbor - 2nd Triangle, First Row, Top Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 1 + 1]);
                            // Right Neighbor - First Triangle, First Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4]);
                            // Interior Neighbor - Last Triangle - 1, First Row, Back Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                    }
                    // BottomLeftCornerTriangle -- DEBUG COLOR: Blue
                    else if (triIndex == trianglesPerFace - trianglesAcross)
                    {
                        // Self - Debug Coloring
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[1]);

                        // Forward
                        if (currentFace == 0)
                        {
                            // Interior Neighbor - Second Triangle, Last Row, Front Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + lastRow + 1]);
                            // Bottom Neighbor - Second Triangle, First Row, Bottom Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 + 1]);
                            // Left Neighbor - Last Triangle, Last Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 5 - 1]);
                        }

                        // Up
                        else if (currentFace == 1)
                        {
                            // Interior Neighbor - Second Triangle, Last Row, Top Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + lastRow + 1]);
                            // Bottom Neighbor - Second Triangle, First Row, Front Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 0 + 1]);
                            // Left Neighbor - Last Triangle, Last Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 + trianglesAcross - 1]);
                        }

                        // Down
                        else if (currentFace == 2)
                        {
                            // Interior Neighbor - Second Triangle, Last Row, Bottom Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + lastRow + 1]);
                            // Bottom Neighbor - Second Triangle, First Row, Front Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 + lastRow]);
                            // Left Neighbor - Last Triangle, Last Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 6 - 2]);
                        }
                        
                        // Left
                        else if (currentFace == 3)
                        {
                            // Interior Neighbor - Second Triangle, Last Row, Left Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + lastRow + 1]);
                            // Bottom Neighbor - Last Triangle, First Row, Bottom Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 + trianglesAcross - 1]);
                            // Left Neighbor - Last Triangle, Last Row, Front Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 1 - 1]);
                        }
                        
                        // Right
                        else if (currentFace == 4)
                        {
                            // Interior Neighbor - Second Triangle, Last Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + lastRow + 1]);
                            // Bottom Neighbor - First Triangle, Last Row, Bottom Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 - trianglesAcross]);
                            // Left Neighbor - Last Triangle, Last Row, Front Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 6 - 1]);
                        }
                        
                        // Back
                        else if (currentFace == 5)
                        {
                            // Interior Neighbor - Second Triangle, Last Row, Right Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + lastRow + 1]);
                            // Bottom Neighbor - Second Last Triangle, Last Row, Bottom Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 - 2]);
                            // Left Neighbor - Last Triangle, Last Row, Left Face
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 - 1]);
                        }
                    }
                    // Top Triangle -- DEBUG COLOR: Yellow
                    else if (triIndex < trianglesAcross && IMathf.IsOdd(triIndex))
                    {
                        // Self - Debug Coloring
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[2]);

                        // Forward
                        if (currentFace == 0)
                        {
                            // Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 1  + lastRow + triIndex - 1]);
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                        
                        // Up
                        else if (currentFace == 1)
                        {
                            // Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 6 - triIndex - lastRow]);
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                        
                        // Down
                        else if (currentFace == 2)
                        {
                            // Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 0 + lastRow + triIndex - 1]);
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                        
                        // Left
                        else if (currentFace == 3)
                        {
                            // Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 - triIndex / 2 * trianglesAcross - 1]);
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        else if (currentFace == 4)
                        {
                            // Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 1 + trianglesAcross * (triIndex / 2)]);
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                        
                        else if (currentFace == 5)
                        {
                            // Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 - triIndex - lastRow]);
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                    }
                    // Left Triangle -- DEBUG COLOR: Cyan
                    else if (triIndex % trianglesAcross == 0)
                    {
                        // Self
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[3]);

                        // Forward
                        if (currentFace == 0)
                        {
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + trianglesAcross + 1]);
                            // Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 + triIndex + trianglesAcross - 1]);
                        }
                        
                        // Up
                        else if (currentFace == 1)
                        {
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + trianglesAcross + 1]);
                            // Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 + triIndex * 2 / trianglesAcross + 1]);
                        }
                        
                        // Down
                        else if (currentFace == 2)
                        {
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + trianglesAcross + 1]);
                            // Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 5 - (triIndex * 2) / trianglesAcross - 2]);
                        }
                        
                        // Left
                        else if (currentFace == 3)
                        {
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + trianglesAcross + 1]);
                            // Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 0 + triIndex + trianglesAcross - 1]);
                        }

                        // Right
                        else if (currentFace == 4)
                        {
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + trianglesAcross + 1]);
                            // Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 5 + triIndex + trianglesAcross - 1]);
                        }

                        // Back
                        else if (currentFace == 5)
                        {
                            // Interior Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Interior Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + trianglesAcross + 1]);
                            // Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 + triIndex + trianglesAcross - 1]);
                        }
                    }
                    // Right Triangle -- DEBUG COLOR: Magenta
                    else if (triIndex % trianglesAcross == trianglesAcross - 1)
                    {
                        // Self - Debug Coloring
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[4]);

                        // Forward
                        if (currentFace == 0)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - trianglesAcross - 1]);
                            // Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 + triIndex - trianglesAcross + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Up
                        else if (currentFace == 1)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - trianglesAcross - 1]);
                            // Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 - (triIndex * 2) / trianglesAcross - lastRow]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Down
                        else if (currentFace == 2)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - trianglesAcross - 1]);
                            // Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 + triIndex * 2 / trianglesAcross + lastRow - 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Left
                        else if (currentFace == 3)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - trianglesAcross - 1]);
                            // Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 5 + triIndex - trianglesAcross + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Right
                        else if (currentFace == 4)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - trianglesAcross - 1]);
                            // Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 0 + triIndex - trianglesAcross + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Back
                        else if (currentFace == 5)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - trianglesAcross - 1]);
                            // Right Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 4 + triIndex - trianglesAcross + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                    }
                    // Bottom Triangle -- DEBUG COLOR: Green
                    else if (triIndex > lastRow && IMathf.IsEven(triIndex))
                    {
                        // Self - Debug Coloring
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[5]);

                        // Forward
                        if (currentFace == 0)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 + triIndex - lastRow + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Up
                        else if (currentFace == 1)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 0 + triIndex - lastRow + 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Down
                        else if (currentFace == 2)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 6 - triIndex - 2 + lastRow]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Left
                        else if (currentFace == 3)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 2 + triIndex % trianglesAcross / 2 * trianglesAcross + trianglesAcross - 1]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Right
                        else if (currentFace == 4)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 - triIndex % trianglesAcross / 2 * trianglesAcross - trianglesAcross]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }

                        // Back
                        else if (currentFace == 5)
                        {
                            // Interior Top Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                            // Bottom Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * 3 - triIndex + lastRow - 2]);
                            // Interior Left Neighbor
                            ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                        }
                    }
                    // Even Interior Triangle -- DEBUG COLOR: White
                    else if (IMathf.IsEven(triIndex))
                    {
                        // Self
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[6]);

                        // Interior Top Neighbor
                        ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                        // Interior Bottom Neighbor
                        ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + trianglesAcross + 1]);
                        // Interior Left Neighbor
                        ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                    }
                    // Odd Interior Triangle -- DEBUG COLOR: Gray
                    else
                    {
                        // Self
                        if (debugProperties.colorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugColors.TriangleColorSet[7]);
                        // Interior Top Neighbor
                        ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - trianglesAcross - 1]);
                        // Interior Right Neighbor
                        ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex + 1]);
                        // Interior Bottom Neighbor
                        ts[trianglesPerFace * currentFace + triIndex].Neighbors.Add(ts[trianglesPerFace * currentFace + triIndex - 1]);
                    }
                }
                return ts;
            }
        }

        private void GetVoronoiCells(Triangle[] tObjs, Vector3[] vs)
        {
            TryLogStart("GetVoronoiCells()");

            // Approach:
            // Given an edge, there are two voronoi cells.
            // Each vertex has 3 edges attached to it
            // When looping thorough edges, the smallest signed angle (angle that has a + cross product with the previous normal vector)
            // - is the next edge of the cell 
            // Somehow remove edges / vertexes from a list when all cells associated with them are found
            List<Edge> voronoiEdges = new List<Edge>();
            HashSet<EdgeInt> map = new HashSet<EdgeInt>(new EdgeIntCompareEdge());
            Dictionary<CellInt, int> cellIntHashSet = new Dictionary<CellInt, int>(new CellIntCompareSegments());
            Dictionary<int, List<int>> vertexAdjacencyGraph = new Dictionary<int, List<int>>();
            foreach(Triangle t in tObjs)
            {
                vertexAdjacencyGraph.Add(t.Index, new List<int>());
                foreach(Triangle n in t.Neighbors)
                {
                    vertexAdjacencyGraph[t.Index].Add(n.Index);

                    EdgeInt e = new EdgeInt(t.Index, n.Index);
                    if (!map.Contains(e))
                    {
                        voronoiEdges.Add(new Edge(t.Centroid, n.Centroid));
                        map.Add(e);
                    }
                }
            }

            Debug.Log(voronoiEdges.Count);

            testEdges = voronoiEdges.ToArray();

            TryLogElapsed("Voronoi Edges Generated");

            List<Cell> voronoiCellList = new List<Cell>();

            for(int a = 0; a < vertexAdjacencyGraph.Count; a++) {
                List<int> start = new List<int>(1);
                start.Add(a);
                RecurseFindCells(start, 1);
            }

            Debug.Log("CellIntCount " + cellIntHashSet.Count);

            CellInt[] cellInts = cellIntHashSet.Keys.ToArray();

            Debug.Log(vs.Length);

            foreach(CellInt cellInt in cellInts) {
                Vector3[] cellVs = new Vector3[cellInt.Points.Count];
                for(int a = 0; a < cellInt.Points.Count; a++) {
                    try {
                        cellVs[a] = vs[cellInt.Points[a]];
                    }
                    catch {
                        Debug.Log("------" + cellInt.Points[a]);
                    }
                }
                voronoiCellList.Add(new Cell(cellVs));
            }

            Debug.Log("Points");
            foreach(int a in cellInts[0].Points) Debug.Log(a);

            TryLogElapsed("Voronoi Cell Objects Generated");

            testCells = voronoiCellList.ToArray();

            TryLogEnd();

            // LOCAL FUNCTIONS
            void RecurseFindCells(List<int> currentCellPartial, int step) {
                List<int> adjacentPoints = vertexAdjacencyGraph[currentCellPartial.Last()];
                for(int a = 0; a < adjacentPoints.Count; a++) {
                    if (step + 1 < 7) {
                        if (step > 1) {
                            List<int> nextCellPartial = new List<int>(currentCellPartial.Count + 1);
                            foreach(int v in currentCellPartial) if (v != currentCellPartial[currentCellPartial.Count - 2]) nextCellPartial.Add(v);
                            nextCellPartial.Add(adjacentPoints[a]);
                            RecurseFindCells(nextCellPartial, step + 1);
                        }
                        else {
                            List<int> nextCellPartial = new List<int>(currentCellPartial.Count + 1);
                            foreach(int v in currentCellPartial) nextCellPartial.Add(v);
                            nextCellPartial.Add(adjacentPoints[a]);
                            RecurseFindCells(nextCellPartial, step + 1);
                        }
                    }
                    else if (currentCellPartial[0] == currentCellPartial.Last()) {
                        CellInt newCell = new CellInt(currentCellPartial);
                        if (!cellIntHashSet.ContainsKey(newCell)) cellIntHashSet.Add(newCell, 1);
                    }
                }
            }
        }

        private void BuildGameObjects(Vector3[] vertices, int[] triangles, Triangle[] tObjs, Color[] colors)
        {
            CubeSphereMesh = new GameObject("CubeSphereMesh");

            for(int tIndex = 0; tIndex < tObjs.Length; tIndex++)
            {
                GameObject tObj = new GameObject("Triangle " + tIndex);
                tObj.transform.parent = CubeSphereMesh.transform;

                MeshFilter meshFilter = tObj.AddComponent<MeshFilter>();
                Mesh mesh = new Mesh();
                Vector3[] pts = tObjs[tIndex].Points;
                mesh.vertices = pts;

                Vector3 triangleSurfaceNormal = Vector3.Cross(pts[2] - pts[0], pts[1] - pts[0]);

                mesh.triangles = Vector3.Dot(pts[0], triangleSurfaceNormal) < 0 ? new int[3] { 0, 1, 2 } : new int[3] { 0, 2, 1 };
                if (debugProperties.colorTriangles) mesh.colors = new Color[3] { colors[tIndex * 3 + 0], colors[tIndex * 3 + 1], colors[tIndex * 3 + 2] };

                meshFilter.sharedMesh = mesh;
                meshFilter.sharedMesh.RecalculateNormals();

                MeshRenderer meshRenderer = tObj.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Globe/Map");

                meshRenderer.enabled = !debugProperties.disableSphereMesh;

                DebugTriangle dt = tObj.AddComponent<DebugTriangle>();
                dt.Neighbors = tObjs[tIndex].Neighbors;
            }
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
            public bool colorTriangles { get; private set; }
            public bool logTime { get; private set; }
            public bool disableSphereMesh { get; private set; }
            public bool disableSphereTransformation { get; private set; }

            public DebugProperties(bool ct, bool lt, bool dsm, bool dst)
            {
                colorTriangles = ct;
                logTime = lt;
                disableSphereMesh = dsm;
                disableSphereTransformation = dst;
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


