using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class VoronoiSphere
{
    private int resolution;
    private float jitter;
    private Vector3[] directions;

    public GameObject VoronoiSphereMesh { get; private set; }

    public VoronoiSphere(int resolution, float jitter)
    {
        this.resolution = resolution;
        this.jitter = jitter;
        this.directions = new Vector3[6] { Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back };

        Generate();
    }

    private void Generate()
    {
        Vector3[] vertices;
        int[] triangles;

        GetVerticesAndTriangles(out vertices, out triangles);
        BuildGameObject(vertices, triangles);
    }

    private void GetVerticesAndTriangles(out Vector3[] vertices, out int[] triangles)
    {
        Dictionary<Vector3, List<int>> map = new Dictionary<Vector3, List<int>>();
        List<int> ts = new List<int>();

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

                    // If map already contains the vertex, that means that it is a border vertex, i to the map
                    if (!map.ContainsKey(pointOnSphere)) map.Add(pointOnSphere, new List<int>());
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

        // Collapse Vertices / Triangles
        foreach(KeyValuePair<Vector3, List<int>> kvp in map)
        {
            // If there is more than one index per vertex, that means that the vertex is shared with another triangle i.e. border vertex.
            // We need to collapse the vertex (by replacing all the triangles with the first instance of i)
            foreach (int v in kvp.Value) Debug.Log($"{kvp.Key}: {v}");
            
            for(int a = 0; a < kvp.Value.Count; a++)
            {
                int[] indexes = Enumerable.Range(0, ts.Count).Where(i => ts[i] == kvp.Value[a]).ToArray();
                for(int b = 0; b < indexes.Length; b++)
                {
                    ts[indexes[b]] = kvp.Value[0];
                }
            }
        }

        foreach (int t in ts) Debug.Log(t);

        Debug.Log(ts.Max());
        Debug.Log(map.Count);

        vertices = map.Keys.ToArray();
        triangles = ts.ToArray();
    }

    private void BuildGameObject(Vector3[] vertices, int[] triangles)
    {
        VoronoiSphereMesh = new GameObject("Mesh");

        MeshFilter meshFilter = VoronoiSphereMesh.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.vertices = vertices;
        meshFilter.sharedMesh.triangles = triangles;
        meshFilter.sharedMesh.RecalculateNormals();

        MeshRenderer meshRenderer = VoronoiSphereMesh.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Globe/Map");
    }

    public static Vector3 GetTriangleCentroid(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return new Vector3((v1.x + v2.x + v3.x) / 3f, (v1.y + v2.y + v3.y) / 3f, (v1.z + v2.z + v3.z) / 3f);
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
