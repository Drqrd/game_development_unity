using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("CubeSphere")]
    [Tooltip("Specifies the resolution of the cube sphere.")]
    [SerializeField] [Range(2,256)] private int resolution = 2;


    private CubeSphere cubeSphere;
    private GameObject meshObject;

    private void Start()
    {
        cubeSphere = new CubeSphere(resolution);
        
        // GenerateMesh();
    }

    private void GenerateMesh()
    {
        meshObject = new GameObject("Mesh");
        meshObject.transform.parent = this.transform;
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Assets/Resources/Materials/Globe/Map");

        meshFilter.sharedMesh = cubeSphere.CubeSphereMesh;

    }
}
