using UnityEngine;

using IDebug;
using Generation.Voronoi;

public class World : MonoBehaviour
{
    [Header("Log Time")]
    [Tooltip("When enabled, will output the times for the given script.")]
    [SerializeField] private bool logWorld;
    [SerializeField] private bool logVoronoiSphere;
    [Header("VoronoiSphere")]
    [Tooltip("Specifies the resolution of the voronoi sphere initial vertices.")]
    [SerializeField] [Range(2,256)] private int resolution = 2;
    [Tooltip("Adds randomization in points after voronoi cells are calculated.")]
    [SerializeField] [Range(0f, 1f)] private float jitter = 0f;

    private Sphere voronoiSphere;
    private TimeTracker debugTimeTracker;

    private void Start()
    {
        debugTimeTracker = (logWorld) ? new TimeTracker() : null;
        voronoiSphere = new Sphere(resolution, jitter, logVoronoiSphere);

        voronoiSphere.CubeSphereMesh.transform.parent = this.transform;
        // voronoiSphere.VoronoiSphereMesh.transform.parent = this.transform;
    }
}
