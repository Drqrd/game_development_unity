using UnityEngine;

public class CubeSphere
{
    private int resolution;
    private bool normalize;
    private Vector3[] directions;
    public Mesh[] Meshes { get; private set; }
    public GameObject CubeSphereMesh { get; private set; }

    public CubeSphere(int resolution, bool normalize)
    {
        this.resolution = resolution;
        this.normalize = normalize;
        this.directions = new Vector3[6] { Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back };

        this.Meshes = new Mesh[6];

        GenerateMesh();
        BuildGameObject();
    }

    public void GenerateMesh()
    {
        // For each direction, make a face
        for(int f = 0; f < directions.Length; f++)
        {
            Vector3 localUp = directions[f];
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);
            
            Vector3[] vertices = new Vector3[resolution * resolution];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

            int triIndex = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Vertices
                    int i = y * resolution + x;
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                    Vector3 pointOnSphere = PointOnCubeToPointOnSphere(pointOnCube);
                    vertices[i] = pointOnSphere;

                    // Triangles
                    if (x != resolution - 1 && y != resolution - 1)
                    {
                        triangles[triIndex] = i;
                        triangles[triIndex + 1] = i + resolution + 1;
                        triangles[triIndex + 2] = i + resolution;

                        triangles[triIndex + 3] = i;
                        triangles[triIndex + 4] = i + 1;
                        triangles[triIndex + 5] = i + resolution + 1;
                        triIndex += 6;
                    }
                }
            }

            Meshes[f] = new Mesh();
            Meshes[f].vertices = vertices;
            Meshes[f].triangles = triangles;
            Meshes[f].RecalculateNormals();
        }
    }

    private void BuildGameObject()
    {
        CubeSphereMesh = new GameObject("Mesh");
        for(int a = 0; a < Meshes.Length; a++)
        {
            GameObject meshObject = new GameObject("Face " + (a + 1));
            meshObject.transform.parent = CubeSphereMesh.transform;
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = Meshes[a];

            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Globe/Map");
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
}
