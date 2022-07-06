using UnityEngine;

public class VoronoiCell
{
    public Vector3 Center {get; private set; }
    public Vector3[] Vertices { get; private set; }
    public int[] Triangles { get; private set; }
    public VoronoiCell Neighbors { get; set; }

    public VoronoiCell(Vector3 center, Vector3[] vertices)
    {
        Center = center;
        Vertices = vertices;
        Triangulate();
    }

    private void Triangulate()
    {

    }
}
