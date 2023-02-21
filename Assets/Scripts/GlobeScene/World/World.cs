using System.Collections.Generic;
using UnityEngine;

using IDebug;
using Generation.Voronoi;
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
    
    [SerializeField] [Range(1,10)] private int pContinentNumber = 1;

    [Header("Height")]
    [Tooltip("How many times the heights blend.")]
    [SerializeField] [Range(0,8)] private int blendTimes = 1;
    [Tooltip("How much neighboring cells are taken into account.")]
    [SerializeField] [Range(0f,2f)] private float blendStrength = 0.5f;

    [Tooltip("Range of noise for continental points.")]
    [SerializeField] [Range(0f,.25f)] private float hContinentNoiseRange = 0f;
    [Tooltip("At -1, noise only goes negative, inverse applies.")]
    [SerializeField] [Range(-1f,1f)] private float hContinentNoiseBias = 0f;
    [Tooltip("Range of noise for oceanic points.")]
    [SerializeField] [Range(0f,.25f)] private float hOceanNoiseRange = 0f;
    [Tooltip("At -1, noise only goes negative, inverse applies.")]
    [SerializeField] [Range(-1f,1f)] private float hOceanNoiseBias = 0f;
    [Header("Evaporation")]
    [SerializeField] private Vector3 eSeed;
    [SerializeField] [Range(0.25f, 2f)] private float eFrequency = 1f;
    [Header("Temperature")]
    [Tooltip("Amount of space from 0 to tmpLatitudeBuffer before temperature starts falling off, from equator to pole.")]
    [SerializeField] [Range(0f,.25f)] private float tmpLatitudeBuffer = 0f;
    [Tooltip("Amount of distance from 0 to tmpHeightBuffer before temperature starts falling off, from sea level to max height.")]
    [SerializeField] [Range(0f,0.5f)] private float tmpHeightBuffer = 0f;
    [Tooltip("Bias for continents.")]
    [SerializeField] [Range(-1f,1f)] private float tmpContinentBias = 0f;
    [Tooltip("Noise range for continents.")]
    [SerializeField] [Range(0f,.25f)] private float tmpContinentNoiseRange = 0f;
    [Tooltip("Bias for oceans.")]
    [SerializeField] [Range(-1f,1f)] private float tmpOceanBias = 0f;
    [Tooltip("Noise range for oceans.")] 
    [SerializeField] [Range(0f,.25f)] private float tmpOceanNoiseRange = 0f;



    [Header("External Property References")]
    [SerializeField] private Generation.Voronoi.Debug.SphereProperties sphereDebugProperties;
    private Sphere voronoiSphere;

    // Used in GlobeCameraController to get the bounds of the sphere
    public Sphere VoronoiSphere { get { return voronoiSphere; } }

    private void Start()
    {
        debugTimeTracker = (logWorld) ? new TimeTracker() : null;

        voronoiSphere = new Sphere(resolution, jitter, sphereDebugProperties);

        Maps.GeneratePlates(voronoiSphere, plateNumber, oToCRatio, pContinentNumber, logMaps);
        Maps.GenerateHeight(voronoiSphere, hContinentNoiseRange, hContinentNoiseBias, hOceanNoiseRange, hOceanNoiseBias, blendStrength, blendTimes, logMaps);
        Maps.GenerateEvaporation(voronoiSphere, eSeed, eFrequency);
        Maps.GenerateTemperature(voronoiSphere, tmpLatitudeBuffer, tmpHeightBuffer, tmpContinentNoiseRange, tmpContinentBias, tmpOceanNoiseRange, tmpOceanBias);
        Maps.GenerateHumidity(voronoiSphere);
        Maps.GenerateBiome(voronoiSphere);

        sphereDebugProperties.Initialize();
        sphereDebugProperties.Colors.GenerateVColors(voronoiSphere.Cells, voronoiSphere.Plates);
        sphereDebugProperties.Colors.ApplyVColor(voronoiSphere.Cells);
        
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
            sphereDebugProperties.Colors.ApplyVColor(voronoiSphere.Cells);
        }

        if(sphereDebugProperties.CurrentSimulatedTime != sphereDebugProperties.SimulatedTime) {
            sphereDebugProperties.CurrentSimulatedTime = sphereDebugProperties.SimulatedTime;
            sphereDebugProperties.Colors.AdjustVColor(voronoiSphere.Cells);
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
