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

        public GameObject MeshObject { get; private set; }
        public Cell[] VoronoiCells { get; private set; }

        public Sphere(int resolution, float jitter, DebugProperties debugProperties)
        {
            this.resolution = resolution;
            this.jitter = jitter;
            this.debugProperties = debugProperties;

            this.directions = new Vector3[6] { Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back };

            this.MeshObject = new GameObject("Sphere Mesh");

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
            GetVoronoiCells(triangleObjs);
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

        private void GetVoronoiCells(Triangle[] tObjs)
        {

            TryLogStart("GetVoronoiCells()");
            
            HashSet<int[]> dcelEdgeSet = new HashSet<int[]>(new HalfEdge.HalfEdgeCompare());

            int cnt = 0;
            foreach(Triangle t in tObjs) {
                foreach(Triangle n in t.Neighbors) {
                    int[] key = new int[2];

                    key[0] = t.Index < n.Index ? t.Index : n.Index;
                    key[1] = key[0] == t.Index ? n.Index : t.Index;

                    if (!dcelEdgeSet.Contains(key)) {
                        dcelEdgeSet.Add(key);
                    }
                    cnt++;
                }
            }

            // Debug.Log("total: " + cnt);
            // Debug.Log("set: " + dcelEdgeSet.Count);

            int[][] dcelEdgeArr = dcelEdgeSet.ToArray();

            Dictionary<int[],HalfEdge> dcelDict = new Dictionary<int[],HalfEdge>( new HalfEdge.HalfEdgeCompare());

            foreach(int[] key in dcelEdgeArr) {
                int[] selfKey = new int[2], twinKey = new int[2];

                selfKey[0] = key[0];
                selfKey[1] = key[1];
                twinKey[0] = key[1];
                twinKey[1] = key[0];

                int k0 = selfKey[0], k1 = selfKey[1];

                Triangle key0 = tObjs[key[0]];
                Triangle key1 = tObjs[key[1]];

                Triangle[] nextTriangles = key1.Neighbors.Where(neighbor => neighbor.Index != key0.Index).ToArray();
                Triangle[] prevTriangles = key0.Neighbors.Where(neighbor => neighbor.Index != key1.Index).ToArray();

                int selfNextIndex = DetermineSide(key0.Centroid, key1.Centroid, nextTriangles[0].Centroid) > 0 ? 0 : 1;
                int twinPrevIndex = selfNextIndex == 0 ? 1 : 0;

                int selfPrevIndex = DetermineSide(key1.Centroid, key0.Centroid, prevTriangles[0].Centroid) < 0 ? 0 : 1;
                int twinNextIndex = selfPrevIndex == 0 ? 1 : 0;
 
                int[] selfNextKey = new int[2], selfPrevKey = new int[2], twinNextKey = new int[2], twinPrevKey = new int[2];
                selfNextKey[0] = key1.Index;
                selfNextKey[1] = nextTriangles[selfNextIndex].Index;
                twinPrevKey[0] = nextTriangles[twinPrevIndex].Index;
                twinPrevKey[1] = key1.Index;

                twinNextKey[0] = key0.Index;
                twinNextKey[1] = prevTriangles[twinNextIndex].Index;
                selfPrevKey[0] = prevTriangles[selfPrevIndex].Index;
                selfPrevKey[1] = key0.Index;
    
                // Check if current edge exists in dictionary
                if (!dcelDict.ContainsKey(selfKey)) {
                    dcelDict.Add(selfKey, new HalfEdge(k0,k1));
                }
                // Check if current edge next is assigned
                if (dcelDict[selfKey].NextEdge == null) {
                    if (!dcelDict.ContainsKey(selfNextKey)) {
                        dcelDict.Add(selfNextKey, new HalfEdge(selfNextKey[0],selfNextKey[1]));
                    }
                    dcelDict[selfKey].NextEdge = dcelDict[selfNextKey];
                }
                // Check if next edge prev is assigned
                if (dcelDict[selfNextKey].PrevEdge == null) {
                    dcelDict[selfNextKey].PrevEdge = dcelDict[selfKey];
                }
                if (dcelDict[selfKey].PrevEdge == null) {
                    if (!dcelDict.ContainsKey(selfPrevKey)) {
                        dcelDict.Add(selfPrevKey, new HalfEdge(selfPrevKey[0],selfPrevKey[1]));
                    }
                    dcelDict[selfKey].PrevEdge = dcelDict[selfPrevKey];
                }
                if (dcelDict[selfPrevKey].NextEdge == null) {
                    dcelDict[selfPrevKey].NextEdge = dcelDict[selfKey];
                }

                // Same but with twin keys
                if (!dcelDict.ContainsKey(twinKey)) {
                    dcelDict.Add(twinKey, new HalfEdge(k1,k0));
                }
                if (dcelDict[twinKey].PrevEdge == null) {
                    if (!dcelDict.ContainsKey(twinPrevKey)) {
                        dcelDict.Add(twinPrevKey, new HalfEdge(twinPrevKey[0],twinPrevKey[1]));
                    }
                    dcelDict[twinKey].PrevEdge = dcelDict[twinPrevKey];
                }
                if (dcelDict[twinPrevKey].NextEdge == null) {
                    dcelDict[twinPrevKey].NextEdge = dcelDict[twinKey];
                }
                if (dcelDict[twinKey].NextEdge == null) {
                    if (!dcelDict.ContainsKey(twinNextKey)) {
                        dcelDict.Add(twinNextKey, new HalfEdge(twinNextKey[0], twinNextKey[1]));
                    }
                    dcelDict[twinKey].NextEdge = dcelDict[twinNextKey];
                }
                if (dcelDict[twinNextKey].PrevEdge == null) {
                    dcelDict[twinNextKey].PrevEdge = dcelDict[twinKey];
                }

                if (dcelDict[selfKey].TwinEdge == null) {
                    dcelDict[selfKey].TwinEdge = dcelDict[twinKey];
                }
                if (dcelDict[twinKey].TwinEdge == null) {
                    dcelDict[twinKey].TwinEdge = dcelDict[selfKey];
                }
            }

            HalfEdge[] dcel = dcelDict.Values.ToArray();

            TryLogElapsed("DCEL Generated");
            
            List<Cell> voronoiCells = new List<Cell>();
            LinkedList<List<HalfEdge>> voronoiPartials = new LinkedList<List<HalfEdge>>();

            voronoiPartials.AddFirst(new List<HalfEdge>());
            voronoiPartials.First().Add(dcel[0]);

            voronoiPartials.AddLast(new List<HalfEdge>());
            voronoiPartials.Last().Add(dcel[0].TwinEdge);

            dcelEdgeSet.Clear();

            /*
            foreach(HalfEdge halfEdge in dcel) {
                Debug.Log(halfEdge.A + ", " + halfEdge.B);
                if (halfEdge.NextEdge == null) {
                    Debug.Log("NextEdge null");
                }
                if (halfEdge.PrevEdge == null) {
                    Debug.Log("PrevEdge null");
                }
            }
            */

            while(voronoiPartials.Count > 0) {
                List<HalfEdge> currentPartial = voronoiPartials.First();

                int[] key = new int[2];
                key[0] = currentPartial.Last().A;
                key[1] = currentPartial.Last().B;    
                
                // Debug.Log(currentPartial.Last().A + ", " + currentPartial.Last().B);
                // Debug.Log("NextEdge: " + currentPartial.Last().NextEdge.A + ", " + currentPartial.Last().NextEdge.B);
                // Debug.Log("PrevEdge: " + currentPartial.Last().PrevEdge.A + ", " + currentPartial.Last().PrevEdge.B);
                // Debug.Log("TwinEdge: " + currentPartial.Last().TwinEdge.A + ", " + currentPartial.Last().TwinEdge.B);
                
                if (!dcelEdgeSet.Contains(key)) {
                    // If cell completed
                    if (currentPartial.Last().NextEdge.B == currentPartial.First().A) {
                        List<Vector3> cellPts = new List<Vector3>();
                        foreach(HalfEdge edge in currentPartial) {
                            cellPts.Add(tObjs[edge.A].Centroid);
                            int[] keyToAdd = new int[2];
                            keyToAdd[0] = edge.A;
                            keyToAdd[1] = edge.B;
                            dcelEdgeSet.Add(keyToAdd);
                        }
                        cellPts.Add(tObjs[currentPartial.Last().NextEdge.A].Centroid);

                        voronoiCells.Add(new Cell(cellPts.ToArray()));
                    }
                    else {
                        voronoiPartials.First().Add(currentPartial.Last().NextEdge);
                        int[] twinKey = new int[2];
                        twinKey[0] = currentPartial.Last().TwinEdge.A;
                        twinKey[1] = currentPartial.Last().TwinEdge.B;
                        if (!dcelEdgeSet.Contains(twinKey)) {
                            voronoiPartials.AddLast(new List<HalfEdge>());
                            voronoiPartials.Last().Add(currentPartial.Last().NextEdge.TwinEdge);
                        }
                    }
                }
                else {
                    voronoiPartials.RemoveFirst();
                }
            }

            VoronoiCells = voronoiCells.ToArray();

            TryLogElapsed("Voronoi Cell Objects Generated");

            // Debug.Log("VoronoiCell Length: " + voronoiCells.Count);
            // Debug.Log("TriangleObj Length: " + tObjs.Length);

            TryLogEnd();

            // LOCAL FUNCTIONS

           float DetermineSide(Vector3 b, Vector3 c, Vector3 x) {
                // https://math.stackexchange.com/questions/214187/point-on-the-left-or-right-side-of-a-plane-in-3d-space
                // Original documentation states the 1st point A should be subtracted from
                // the other points, but since A would be Vector3.zero, not needed.
                
                // Gets determinant from the points
                return Mathf.Sign(b.x * c.y * x.z + b.y * c.z * x.x + b.z * c.x * x.y - b.z * c.y * x.x - b.y * c.x * x.z - b.x * c.z * x.y);
            }
        }

        private void BuildGameObjects(Vector3[] vertices, int[] triangles, Triangle[] tObjs, Color[] colors)
        {
            GameObject cubeSphereMesh = new GameObject("Cube Sphere Mesh");
            cubeSphereMesh.transform.parent = MeshObject.transform;

            for(int tIndex = 0; tIndex < tObjs.Length; tIndex++) {
                GameObject tObj = new GameObject("Triangle " + tIndex);
                tObj.transform.parent = cubeSphereMesh.transform;

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

                // DebugTriangle dt = tObj.AddComponent<DebugTriangle>();
                // dt.Neighbors = tObjs[tIndex].Neighbors;
            }

            GameObject voronoiSphereMesh = new GameObject("Voronoi Sphere Mesh");
            voronoiSphereMesh.transform.parent = MeshObject.transform;
            
            for(int vIndex = 0; vIndex < VoronoiCells.Length; vIndex++) {
                GameObject vObj = new GameObject("Voronoi Cell " + vIndex);
                vObj.transform.parent = voronoiSphereMesh.transform;

                MeshFilter meshFilter = vObj.AddComponent<MeshFilter>();
                Mesh mesh = new Mesh();
                Color[] clrs = new Color[VoronoiCells[vIndex].TVertices.Length];
                Color randColor = UnityEngine.Random.ColorHSV();
                for(int a = 0; a < clrs.Length; a++) {
                    clrs[a] = randColor;
                }
                mesh.vertices = VoronoiCells[vIndex].TVertices;
                mesh.triangles = VoronoiCells[vIndex].Triangles;
                mesh.colors = clrs;
                
                meshFilter.sharedMesh = mesh;
                meshFilter.sharedMesh.RecalculateNormals();

                MeshRenderer meshRenderer = vObj.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Globe/Map");

                meshRenderer.enabled = !debugProperties.disableSphereMesh;

                // DebugCell dt = vObj.AddComponent<DebugCell>();
                // dt.Vertices = VoronoiCells[vIndex].Vertices;
            }

            switch (debugProperties.displayedSphereMesh) {
                case DebugProperties.SphereMeshType.cubeSphereMesh: 
                    voronoiSphereMesh.gameObject.SetActive(false);
                    break;
                case DebugProperties.SphereMeshType.voronoiSphereMesh:
                    cubeSphereMesh.gameObject.SetActive(false);
                    break;
                default:
                    Debug.LogError("SphereMeshType INCORRECT ENUM VALUE");
                    break;
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
            public enum SphereMeshType {
                cubeSphereMesh,
                voronoiSphereMesh
            }

            public bool colorTriangles { get; private set; }
            public bool logTime { get; private set; }
            public SphereMeshType displayedSphereMesh {get; private set;}
            public bool disableSphereMesh { get; private set; }
            public bool disableSphereTransformation { get; private set; }

            public DebugProperties(bool ct, bool lt, SphereMeshType displaySm, bool disableSm, bool dst)
            {
                colorTriangles = ct;
                logTime = lt;
                displayedSphereMesh = displaySm;
                disableSphereMesh = disableSm;
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

    class HalfEdge {
        public int A { get; private set; }
        public int B { get; private set; }
        public int Face { get; set; }
        public HalfEdge PrevEdge { get; set; }
        public HalfEdge TwinEdge { get; set; }
        public HalfEdge NextEdge { get; set; }
        public HalfEdge(int a, int b) {

            this.A = a;
            this.B = b;

            this.PrevEdge = null;
            this.TwinEdge = null;
            this.NextEdge = null;
        }

        public sealed class HalfEdgeCompare : IEqualityComparer<int[]>
        {
            public bool Equals(int[] edgeOne, int[] edgeTwo) {
                return edgeOne[0] == edgeTwo[0] && edgeOne[1] == edgeTwo[1];
            }

            // Cantor pairing function
            // https://en.wikipedia.org/wiki/Pairing_function
            public int GetHashCode(int[] edge) {
                return (int)(0.5f * (edge[0] + edge[1]) * (edge[0] + edge[1] + 1) + edge[1]);
            }
        }
    }
}


