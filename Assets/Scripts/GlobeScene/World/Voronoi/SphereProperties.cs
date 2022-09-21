using UnityEngine;

namespace Generation.Voronoi.Debug {
    public class SphereProperties : MonoBehaviour
    {
        public enum SphereMeshType {
            triangulation,
            voronoi,
        }
        public enum VoronoiMeshType {
            cells,
            tectonicPlates,
            height,
            humidity,
            temperature,
            biome,
            terrain,
        }

        public SphereMeshType DisplayedMesh { get { return displayedMesh; } }
        public VoronoiMeshType DisplayedVoronoi { get { return displayedVoronoi; } }

        public bool LogTime { get {return logTime; } }
        public bool ConvertToSphere { get { return convertToSphere; } }
        public bool ColorTriangles { get { return colorTriangles; } }
        public bool DisableSphereMesh { get { return disableSphereMesh; } }
        public bool GenerateDebugTriangles { get { return generateDebugTriangles; } }
        public bool GenerateDebugCells { get { return generateDebugCells; } }
        public Gradient LandGradient { get { return landGradient; } }
        public Gradient WaterGradient { get { return waterGradient; } }
        public Gradient HeightGradient { get { return heightGradient; } }
        public Gradient TemperatureGradient { get { return temperatureGradient; } }
        public Gradient HumidityGradient { get { return humidityGradient; } }
        public Gradient TerrainGradient { get { return terrainGradient; } }

        public DColors Colors { get; private set; } 
        
        [Header("Reactive")]
        [Tooltip("Determines displayed sphere mesh.")]
        [SerializeField] SphereMeshType displayedMesh;
        [Tooltip("Controls which type of voronoi mesh is displayed.")]
        [SerializeField] VoronoiMeshType displayedVoronoi;

        [Header("Initialization")]
        [Tooltip("When enabled, will output the times for the Sphere script.")]
        [SerializeField] private bool logTime;
        [Tooltip("Prevents points from being converted to a sphere.")]
        [SerializeField] private bool convertToSphere;
        [Tooltip("Generates Triangles with unique colors depending on its type.")]
        [SerializeField] private bool colorTriangles;
        [Tooltip("Shows triangle properties, such as neighbors.")]
        [SerializeField] private bool generateDebugTriangles;
        [Tooltip("Shows cell properties, such as neighbors.")]
        [SerializeField] private bool generateDebugCells;
        [Tooltip("Disables Mesh Renderers on Sphere.cs")]
        [SerializeField] private bool disableSphereMesh;
        
        [Header("Parameters")]
        [SerializeField] private Gradient waterGradient;
        [SerializeField] private Gradient landGradient;
        [SerializeField] private Gradient heightGradient;
        [SerializeField] private Gradient temperatureGradient;
        [SerializeField] private Gradient humidityGradient;
        [SerializeField] private Gradient terrainGradient;
        // REACTIVE PROPS
        public SphereMeshType CurrentDisplayedMesh { get; set; }
        public VoronoiMeshType CurrentDisplayedVoronoi { get; set; }

        public void Initialize() {
            Colors = new DColors(this);
            CurrentDisplayedMesh = DisplayedMesh;
            CurrentDisplayedVoronoi = DisplayedVoronoi;
        }

        public class DColors
        {
            // Used when showing the colors for faces in triangulation
            public Color[] TriangleColorSet { get; private set; }

            // Voronoi
            public Color[][] VCells { get; set; }
            public Color[][] VTectonicPlates { get; private set; }
            public Color[][] VHeight { get; private set; }
            public Color[] VTemperature { get; private set; }
            public Color[] VHumidity { get; private set; }
            public Color[] VBiome { get; private set; }
            public Color[] VTerrain { get; private set; }

            private SphereProperties sphereProperties;

            public DColors(SphereProperties sphereProperties)
            {
                this.sphereProperties = sphereProperties;

                // Start, End, Top, Left, Right, Down, InteriorEven, InteriorOdd
                TriangleColorSet = new Color[8] { Color.red, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.green, Color.white, Color.gray };
            }

            public void GenerateVColors(Voronoi.Cell[] cells, Plate[] plates) {
                Color[][] vCells = new Color[cells.Length][];
                Color[][] vTectonicPlates = new Color[cells.Length][];
                Color[][] vHeight = new Color[cells.Length][];


                Color[] landPlateColors = new Color[plates.Length];
                Color[] waterPlateColors = new Color[plates.Length];

                for(int a = 0; a < plates.Length; a++) {
                    landPlateColors[a] = sphereProperties.LandGradient.Evaluate(Random.value);
                    waterPlateColors[a] = sphereProperties.WaterGradient.Evaluate(Random.value);
                }

                for(int a = 0; a < cells.Length; a++) {
                    vCells[a] = new Color[cells[a].TVertices.Length];
                    vTectonicPlates[a] = new Color[cells[a].TVertices.Length];
                    vHeight[a] = new Color[cells[a].TVertices.Length];
                    
                    Color cellColor = Random.ColorHSV();
                    Color heightColor = sphereProperties.HeightGradient.Evaluate(Data.GetScaledHeight(cells[a].Properties.Height));
                    
                    for(int b = 0; b < cells[a].TVertices.Length; b++) {
                        vCells[a][b] = cellColor;
                        
                        vTectonicPlates[a][b] = cells[a].Plate.Type == Plate.PlateType.continental ? 
                            landPlateColors[cells[a].Plate.Index] :
                            waterPlateColors[cells[a].Plate.Index];

                        vHeight[a][b] = heightColor;
                    }
                }

                VCells = vCells;
                VTectonicPlates = vTectonicPlates;
                VHeight = vHeight;
            }

            public void ApplyVColor(VoronoiMeshType v, Voronoi.Cell[] cells) {
                for(int a = 0; a < cells.Length; a++) {
                    switch (v) {
                        case VoronoiMeshType.cells:
                            cells[a].MeshRef.colors = VCells[a];
                            break;
                        case VoronoiMeshType.tectonicPlates:
                            cells[a].MeshRef.colors = VTectonicPlates[a];
                            break;
                        case VoronoiMeshType.height:
                            cells[a].MeshRef.colors = VHeight[a];
                            break;
                        default:
                            UnityEngine.Debug.LogError("ApplyVColor: VoronoiMeshType not in enum.");
                            break;
                    }
                }
            }
        }
    }
}
