using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("VoronoiSphere")]
    [Tooltip("Specifies the resolution of the voronoi sphere initial vertices.")]
    [SerializeField] [Range(2,256)] private int resolution = 2;
    [Tooltip("Adds randomization in points after voronoi cells are calculated")]
    [SerializeField] [Range(0f, 1f)] private float jitter = 0f;

    private VoronoiSphere voronoiSphere;

    private void Start()
    {
        voronoiSphere = new VoronoiSphere(resolution, jitter);

        voronoiSphere.VoronoiSphereMesh.transform.parent = this.transform;
    }
}
