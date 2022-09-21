using System.Collections.Generic;
using UnityEngine;

using IDebug;
using Generation.Voronoi;
using LineFunctions;
public class World : BaseObjectMono
{
    [Header("Debug")]
    [Tooltip("When enabled, will output the times for this script.")]
    [SerializeField] private bool logWorld;
    [Tooltip("When enabled, will output the times for map generation.")]
    [SerializeField] private bool logMaps;
    
    [Header("Sphere")]
    [Tooltip("Specifies the resolution of the voronoi sphere initial vertices.")]
    [SerializeField] [Range(2,50)] private int resolution = 2;
    [Tooltip("Adds randomization in points after voronoi cells are calculated.")]
    [SerializeField] [Range(0f, 1f)] private float jitter = 0f;
    
    [Header("Plates")]
    [Tooltip("Number of tectonic plates.")]
    [SerializeField] [Range(10,200)] private int plateNumber = 10;
    [Tooltip("Percentage of oceanic to continental plates")]
    [SerializeField] [Range(0f,1f)] private float oToCRatio = 0.7f;

    [Header("Height")]
    [Tooltip("How far heights blend.")] 
    [SerializeField] [Range(0,4)] private int blendLevel = 3;
    [Tooltip("How much neighboring cells are taken into account.")]
    [SerializeField] [Range(0f,1f)] private float blendStrength = 0.5f;
    [Tooltip("How many times the heights blend.")]
    [SerializeField] [Range(0,4)] private int blendTimes = 1;
    [Tooltip("Range of noise for continental points")]
    [SerializeField] [Range(0f,.25f)] private float continentNoiseRange = 0f;
    [Tooltip("At -1, noise only goes negative, inverse applies.")]
    [SerializeField] [Range(-1f,1f)] private float continentNoiseBias = 0f;
    [Tooltip("Range of noise for oceanic points")]
    [SerializeField] [Range(0f,.25f)] private float oceanNoiseRange = 0f;
    [Tooltip("At -1, noise only goes negative, inverse applies.")]
    [SerializeField] [Range(-1f,1f)] private float oceanNoiseBias = 0f;

    [Header("External Property References")]
    [SerializeField] private Generation.Voronoi.Debug.SphereProperties sphereDebugProperties;
    private Sphere voronoiSphere;

    private void Start()
    {
        debugTimeTracker = (logWorld) ? new TimeTracker() : null;

        voronoiSphere = new Sphere(resolution, jitter, sphereDebugProperties);

        Maps.GeneratePlates(voronoiSphere, plateNumber, oToCRatio, logMaps);
        Maps.GenerateHeight(voronoiSphere, continentNoiseRange, continentNoiseBias, oceanNoiseRange, oceanNoiseBias, blendLevel, blendStrength, blendTimes, logMaps);
        // Maps.GenerateTemperature(voronoiSphere);

        sphereDebugProperties.Initialize();
        sphereDebugProperties.Colors.GenerateVColors(voronoiSphere.Cells, voronoiSphere.Plates);
        sphereDebugProperties.Colors.ApplyVColor(sphereDebugProperties.CurrentDisplayedVoronoi, voronoiSphere.Cells);
        
        voronoiSphere.MeshObject.transform.parent = this.transform;
    }

    private void Update() {
        #if UNITY_EDITOR
        HandleReactiveProperties();
        #endif
    }
    
    private void HandleReactiveProperties() {

        // SPHERE
        if (sphereDebugProperties.CurrentDisplayedMesh != sphereDebugProperties.DisplayedMesh) {
            sphereDebugProperties.CurrentDisplayedMesh = sphereDebugProperties.DisplayedMesh;

            int index = 0;
            foreach (Transform child in voronoiSphere.MeshObject.transform) {
                child.gameObject.SetActive((int)sphereDebugProperties.CurrentDisplayedMesh == index);
                index++;
            }
        }
        if (sphereDebugProperties.CurrentDisplayedVoronoi != sphereDebugProperties.DisplayedVoronoi) {
            sphereDebugProperties.CurrentDisplayedVoronoi = sphereDebugProperties.DisplayedVoronoi;
            sphereDebugProperties.Colors.ApplyVColor(sphereDebugProperties.CurrentDisplayedVoronoi, voronoiSphere.Cells);
        }
    }


    // START FUNCTIONS

    // UPDATE FUNCTIONS

    private void OnDrawGizmosSelected() {
        /*
        #if UNITY_EDITOR
        if (Application.isPlaying) {
            foreach(Plate plate in voronoiSphere.Plates){
                int index = 0;
                foreach(Cell[] border in plate.Borders) {
                    Gizmos.color = plate.debugColors[index];
                    foreach(Cell cell in border) {
                        Gizmos.DrawSphere(cell.Center, 0.01f);
                    }
                    index++;
                }
            }
        }
        #endif
        */    
    }
}
