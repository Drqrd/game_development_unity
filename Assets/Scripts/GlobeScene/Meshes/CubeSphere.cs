using UnityEngine;

public class CubeSphere
{
    private int resolution;
    private Vector3[] directions;

    public Vector3[] Vertices { get; private set; }
    public int[] Triangles { get; private set; }
    public Mesh CubeSphereMesh { get; private set; }

    public CubeSphere(int resolution)
    {
        this.resolution = resolution;
        this.directions = new Vector3[6] { Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back };

        this.Vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        this.Triangles = new int[resolution * resolution * 6];
        this.CubeSphereMesh = new Mesh();

        Build();
    }

    public void Build()
    {
        float step = 1f / resolution;
        Debug.Log(step);

        for(int f = 0; f < )
        
        for(int y = 0; y < resolution + 1; y++)
        {
            for(int x = 0; x < resolution + 1; x++)
            {
                // Vertices
                Debug.Log(new Vector2(x * step, y * step));
            }
        }
    }

    public static Vector3 PointOnCubeToPointOnSphere(Vector3 p)
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;

        float x = p.x * Mathf.Sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
        float y = p.y * Mathf.Sqrt(1 - (x2 + z2) / 2 + (x2 * z2) / 3);
        float z = p.z * Mathf.Sqrt(1 - (x2 + y2) / 2 + (x2 * z2) / 3);

        return new Vector3(x, y, z);
    }
}
