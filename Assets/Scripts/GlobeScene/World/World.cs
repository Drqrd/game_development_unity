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
    [Tooltip("When enabled, will output the times for the Sphere script.")]
    [SerializeField] private bool logVoronoiSphere;
    [Tooltip("Prevents points from being converted to a sphere.")]
    [SerializeField] private bool disableSphereTransformation;
    [Tooltip("Generates Triangles with unique colors depending on its type.")]
    [SerializeField] private bool colorTriangles;
    [Tooltip("Mesh Type for Sphere.cs")]
    [SerializeField] private Sphere.DebugProperties.SphereMeshType displayedSphereMesh;
    [Tooltip("Disables Mesh Renderers on Sphere.cs")]
    [SerializeField] private bool disableSphereMesh;


    private Sphere voronoiSphere;
    private Color[] testColors;

    private void Start()
    {
        debugTimeTracker = (logWorld) ? new TimeTracker() : null;
        
        // Sphere voronoiSphere;
        Sphere.DebugProperties sphereDebugProperties;

        GenerateDebugProperties(out sphereDebugProperties);

        voronoiSphere = new Sphere(resolution, jitter, sphereDebugProperties);

        voronoiSphere.MeshObject.transform.parent = this.transform;
        // voronoiSphere.VoronoiSphereMesh.transform.parent = this.transform;
    }

    private void GenerateDebugProperties(out Sphere.DebugProperties debugSphere)
    {
        debugSphere = new Sphere.DebugProperties(colorTriangles, logVoronoiSphere, displayedSphereMesh, disableSphereMesh, disableSphereTransformation);
    }
}
