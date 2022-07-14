using System.Collections.Generic;
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

    private Color[] color = new Color[] { new Color(0,0,0), new Color(.25f,0,0), new Color(.5f,0,0), new Color(.75f,0,0), new Color(1,0,0),
                                       new Color(0,.25f,0), new Color(0,.5f,0), new Color(0,.75f,0), new Color(0,1,0), new Color(0,0,.25f),
                                       new Color(0,0,.5f), new Color(0,0,.75f), new Color(0,0,1), new Color(.25f,.25f,0), new Color(.5f,.25f,0),
                                       new Color(.75f,.25f,0), new Color(1f,.25f,0), new Color(.25f,.5f,0), new Color(.5f,.5f,0), new Color(.75f,.5f,0)};

    private void Start()
    {
        debugTimeTracker = (logWorld) ? new TimeTracker() : null;
        voronoiSphere = new Sphere(resolution, jitter, logVoronoiSphere);

        voronoiSphere.CubeSphereMesh.transform.parent = this.transform;
        // voronoiSphere.VoronoiSphereMesh.transform.parent = this.transform;
    }

    /*
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {   
            for(int a = 0; a < voronoiSphere.borderVerts.Length; a++)
            {
                Gizmos.color = color[a];
                foreach(Vector3 vv in voronoiSphere.borderVerts[a]) Gizmos.DrawSphere(vv, 0.05f);
            }
        }
    }
    */
}
