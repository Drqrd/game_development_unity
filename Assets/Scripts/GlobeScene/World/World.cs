using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("CubeSphere")]
    [Tooltip("Specifies the resolution of the cube sphere.")]
    [SerializeField] [Range(2,256)] private int resolution = 2;
    [Tooltip("Normalizes the cube to a sphere when true.")]
    [SerializeField] private bool normalize = true;

    private CubeSphere cubeSphere;
    private GameObject meshObject;

    private void Start()
    {
        cubeSphere = new CubeSphere(resolution, normalize);

        meshObject = cubeSphere.CubeSphereMesh;
        meshObject.transform.parent = this.transform;
    }
}
