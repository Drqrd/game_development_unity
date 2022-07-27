using System.Collections.Generic;
using UnityEngine;

using IDebug;
using Generation.Voronoi;

public class World : BaseObjectMono
{
    [Header("Debug")]
    [Tooltip("When enabled, will output the times for this script.")]
    [SerializeField] private bool logWorld;

    [Header("VoronoiSphere")]
    [Tooltip("Specifies the resolution of the voronoi sphere initial vertices.")]
    [SerializeField] [Range(2,256)] private int resolution = 2;
    [Tooltip("Adds randomization in points after voronoi cells are calculated.")]
    [SerializeField] [Range(0f, 1f)] private float jitter = 0f;
    [Header("- Debug")]
    [Tooltip("Generates Triangles with unique vertices for triangle debugging.")]
    [SerializeField] private bool uniqueTriangles;
    [Tooltip("When enabled, will output the times for the Sphere script.")]
    [SerializeField] private bool logVoronoiSphere;

    private Sphere voronoiSphere;

    private void Start()
    {
        debugTimeTracker = (logWorld) ? new TimeTracker() : null;
        
        // Sphere voronoiSphere;
        Sphere.DebugProperties sphereDebugProperties;

        GenerateDebugProperties(out sphereDebugProperties);


        voronoiSphere = new Sphere(resolution, jitter, sphereDebugProperties);

        voronoiSphere.CubeSphereMesh.transform.parent = this.transform;
        // voronoiSphere.VoronoiSphereMesh.transform.parent = this.transform;
    }

    private void GenerateDebugProperties(out Sphere.DebugProperties debugSphere)
    {
        debugSphere = new Sphere.DebugProperties(uniqueTriangles, logVoronoiSphere);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach(Triangle tri in voronoiSphere.debugTs)
            {
                foreach(Triangle n in tri.Neighbors)
                {
                    Gizmos.DrawSphere(n.Centroid, .1f);
                }
            }
        }
    }
}
