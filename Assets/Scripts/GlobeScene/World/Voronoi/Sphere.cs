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
        private Debug.SphereProperties debugProperties;
        private Debug.SphereProperties.SphereMeshType initialSphereMesh;

        public GameObject MeshObject { get; private set; }
        public Voronoi.Cell[] Cells { get; private set; }
        public Plate[] Plates { get; set; }
        public Sphere(int resolution, float jitter, Debug.SphereProperties debugProperties)
        {
            this.resolution = resolution;
            this.jitter = (jitter / 3f) * (1f / (resolution - 1));
            this.debugProperties = debugProperties;
            this.initialSphereMesh = debugProperties.DisplayedMesh;

            this.directions = new Vector3[6] { Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back };

            this.MeshObject = new GameObject("Sphere Mesh");

            debugTimeTracker = (debugProperties.LogTime) ? new TimeTracker() : null;

            Generate();
        }

        private void Generate()
        {
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
                        Vector3 pointOnSphere = !debugProperties.ConvertToSphere ? pointOnCube : PointOnCubeToPointOnSphere(pointOnCube);
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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[0]);

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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[1]);

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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[2]);

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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[3]);

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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[4]);

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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[5]);

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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[6]);

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
                        if (debugProperties.ColorTriangles) for (int col = 0; col < 3; col++) cs.Add(debugProperties.Colors.TriangleColorSet[7]);
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

            // Generate DCEL
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
            
            List<HalfEdge[]> completedVoronoiCells = new List<HalfEdge[]>();
            List<Cell> voronoiCells = new List<Cell>();
            LinkedList<List<HalfEdge>> voronoiPartials = new LinkedList<List<HalfEdge>>();

            voronoiPartials.AddFirst(new List<HalfEdge>());
            voronoiPartials.First().Add(dcel[0]);

            voronoiPartials.AddLast(new List<HalfEdge>());
            voronoiPartials.Last().Add(dcel[0].TwinEdge);

            dcelEdgeSet.Clear();
            
            // Generate Voronoi Cells
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
                        
                        //List<int> cellPts = new List<int>();
                        foreach(HalfEdge edge in currentPartial) {
                            //cellPts.Add(edge.A);
                            int[] keyToAdd = new int[2];
                            keyToAdd[0] = edge.A;
                            keyToAdd[1] = edge.B;
                            dcelEdgeSet.Add(keyToAdd);
                        }
                        //cellPts.Add(currentPartial.Last().NextEdge.A);
                        //voronoiCellInts.Add(cellPts.ToArray());
                        currentPartial.Add(currentPartial.Last().NextEdge);
                        completedVoronoiCells.Add(currentPartial.ToArray());
                        voronoiPartials.RemoveFirst();
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

            TryLogElapsed("Voronoi Cell Objects Generated");
            
            if (jitter > 0f) {
                foreach(Triangle t in tObjs) {
                    t.RandomizeCentroid(jitter);
                }

                TryLogElapsed("Added Jitter to Voronoi Cell Points");
            }

            int cellIndex = 0;
            foreach(HalfEdge[] halfEdgeGroup in completedVoronoiCells) {
                foreach(HalfEdge halfEdge in halfEdgeGroup) {
                    halfEdge.Face = cellIndex;
                }
                cellIndex++;
            }
            
            int index = 0;
            foreach(HalfEdge[] halfEdgeGroup in completedVoronoiCells) {
                Vector3[] vs = new Vector3[halfEdgeGroup.Length];
                
                for(int a = 0; a < halfEdgeGroup.Length; a++) {
                    vs[a] = tObjs[halfEdgeGroup[a].A].Centroid;
                }
                voronoiCells.Add(new Cell(vs, index));
                index++;
            }
            
            index = 0;
            foreach(HalfEdge[] halfEdgeGroup in completedVoronoiCells) {
                HashSet<int> neighbors = new HashSet<int>();
                for(int a = 0; a < halfEdgeGroup.Length; a++) {
                    neighbors.Add(halfEdgeGroup[a].TwinEdge.Face);
                }
                Cell[] actualNeighbors = new Cell[neighbors.Count];
                int[] n = neighbors.ToArray();
                for(int a = 0; a < n.Length; a++) {
                    actualNeighbors[a] = voronoiCells[n[a]];
                }
                voronoiCells[index].Neighbors = actualNeighbors;
                index++;
            }

            Cells = voronoiCells.ToArray();

            TryLogElapsed("Voronoi Cell Data Assigned");

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
                if (debugProperties.ColorTriangles) mesh.colors = new Color[3] { colors[tIndex * 3 + 0], colors[tIndex * 3 + 1], colors[tIndex * 3 + 2] };

                meshFilter.sharedMesh = mesh;
                meshFilter.sharedMesh.RecalculateNormals();

                MeshRenderer meshRenderer = tObj.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Globe/Map");

                meshRenderer.enabled = !debugProperties.DisableSphereMesh;

                if (debugProperties.GenerateDebugTriangles) {
                    Debug.Triangle dt = tObj.AddComponent<Debug.Triangle>();
                    dt.Neighbors = tObjs[tIndex].Neighbors;
                }
            }

            GameObject voronoiSphereMesh = new GameObject("Voronoi Sphere Mesh");
            voronoiSphereMesh.transform.parent = MeshObject.transform;
            
            for(int vIndex = 0; vIndex < Cells.Length; vIndex++) {
                GameObject vObj = new GameObject("Voronoi Cell " + vIndex);
                vObj.transform.parent = voronoiSphereMesh.transform;

                MeshFilter meshFilter = vObj.AddComponent<MeshFilter>();
                Mesh mesh = new Mesh();

                mesh.vertices = Cells[vIndex].TVertices;
                mesh.triangles = Cells[vIndex].Triangles;
                
                meshFilter.sharedMesh = mesh;
                meshFilter.sharedMesh.RecalculateNormals();
                
                Cells[vIndex].MeshRef = mesh;

                MeshRenderer meshRenderer = vObj.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Globe/Map");

                meshRenderer.enabled = !debugProperties.DisableSphereMesh;

                if (debugProperties.GenerateDebugCells) {
                    Debug.Cell dt = vObj.AddComponent<Debug.Cell>();
                    dt.cell = Cells[vIndex];
                } 
            }

            switch (initialSphereMesh) {
                case Debug.SphereProperties.SphereMeshType.triangulation: 
                    voronoiSphereMesh.gameObject.SetActive(false);
                    break;
                case Debug.SphereProperties.SphereMeshType.voronoi:
                    cubeSphereMesh.gameObject.SetActive(false);
                    break;
                default:
                    UnityEngine.Debug.LogError("SphereMeshType INCORRECT ENUM VALUE");
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

            return new Vector3(x, y, z).normalized;
        }
    }

// CLASSES
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


