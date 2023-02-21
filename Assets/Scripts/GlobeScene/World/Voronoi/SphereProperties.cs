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
            evaporation,
            temperature,
            humidity,
            biome,
            terrain,
            aboveWater,
        }

        public SphereMeshType DisplayedMesh { get { return displayedMesh; } }
        public VoronoiMeshType DisplayedVoronoi { get { return displayedVoronoi; } }
        public float SimulatedTime { get {return simulatedTime; } }
        
        public bool LogTime { get {return logTime; } }
        public bool ConvertToSphere { get { return convertToSphere; } }
        public bool ColorTriangles { get { return colorTriangles; } }
        public bool DisableSphereMesh { get { return disableSphereMesh; } }
        public bool GenerateDebugTriangles { get { return generateDebugTriangles; } }
        public bool GenerateDebugCells { get { return generateDebugCells; } }
        public Gradient LandGradient { get { return landGradient; } }
        public Gradient WaterGradient { get { return waterGradient; } }
        public Gradient HeightGradient { get { return heightGradient; } }
        public Gradient EvaporationGradient { get { return evaporationGradient; } }
        public Gradient TemperatureGradient { get { return temperatureGradient; } }
        public Gradient HumidityGradient { get { return humidityGradient; } }
        public Gradient TerrainGradient { get { return terrainGradient; } }

        public Color DesertColor { get { return desertColor; } }
        public Color DrylandsColor { get { return drylandsColor; } }
        public Color GrasslandsColor { get { return grasslandsColor; } }
        public Color TundraColor { get { return tundraColor; } }
        public Color FrozenDesertColor { get { return frozenDesertColor; } }
        public Color DryForestColor { get { return dryForestColor; } }
        public Color ForestColor { get { return forestColor; } }
        public Color RainForestColor { get { return rainForestColor; } }
        public Color ColdForestColor { get { return coldForestColor; } }
        public Color DesertHillsColor { get { return desertHillsColor; } }
        public Color SmallHillsColor { get { return smallHillsColor; } }
        public Color MesaColor { get { return mesaColor; } }
        public Color HillsColor { get { return hillsColor; } }
        public Color TundraHillsColor { get { return tundraHillsColor; } }
        public Color FrozenHillsColor { get { return frozenHillsColor; } }
        public Color LargeHillsColor { get { return largeHillsColor; } }
        public Color MountainsColor { get { return mountainsColor; } }
        public Color WarmOceanColor { get { return warmOceanColor; } }
        public Color ColdOceanColor { get { return coldOceanColor; } }
        public Color GlacialColor { get { return glacialColor; } }

        public DColors Colors { get; private set; } 
        
        [Header("Reactive")]
        [Tooltip("Determines displayed sphere mesh.")]
        [SerializeField] private SphereMeshType displayedMesh;
        [Tooltip("Controls which type of voronoi mesh is displayed.")]
        [SerializeField] private VoronoiMeshType displayedVoronoi;
        [Tooltip("Simulates time, 0 is winter, 1 is summer")]
        [SerializeField] [Range(0f,1f)] private float simulatedTime;

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
        [SerializeField] private Gradient evaporationGradient;
        [SerializeField] private Gradient temperatureGradient;
        [SerializeField] private Gradient humidityGradient;
        [SerializeField] private Gradient terrainGradient;

        [Header("Biome Colors")]
        [SerializeField] private Color desertColor;
        [SerializeField] private Color drylandsColor;
        [SerializeField] private Color grasslandsColor;
        [SerializeField] private Color tundraColor;
        [SerializeField] private Color frozenDesertColor;
        [SerializeField] private Color dryForestColor;
        [SerializeField] private Color forestColor;
        [SerializeField] private Color rainForestColor;
        [SerializeField] private Color coldForestColor;
        [SerializeField] private Color desertHillsColor;
        [SerializeField] private Color smallHillsColor;
        [SerializeField] private Color mesaColor;        
        [SerializeField] private Color hillsColor;
        [SerializeField] private Color tundraHillsColor;
        [SerializeField] private Color frozenHillsColor;
        [SerializeField] private Color largeHillsColor;
        [SerializeField] private Color mountainsColor;
        [SerializeField] private Color warmOceanColor;
        [SerializeField] private Color coldOceanColor;
        [SerializeField] private Color glacialColor;


        // REACTIVE PROPS
        public SphereMeshType CurrentDisplayedMesh { get; set; }
        public VoronoiMeshType CurrentDisplayedVoronoi { get; set; }
        public float CurrentSimulatedTime { get; set; }

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
            public Color[][] VEvaporation { get; private set; }
            public Color[][] VTemperature { get; private set; }
            public Color[][] VHumidity { get; private set; }
            public Color[][] VBiome { get; private set; }
            public Color[] VTerrain { get; private set; }
            public Color[][] VAboveWater { get; private set; }

            private SphereProperties sphereProperties;
            private Color[] biomeColors;

            public DColors(SphereProperties sphereProperties)
            {
                this.sphereProperties = sphereProperties;

                // Start, End, Top, Left, Right, Down, InteriorEven, InteriorOdd
                TriangleColorSet = new Color[8] { Color.red, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.green, Color.white, Color.gray };
                
                this.biomeColors = new Color[] { sphereProperties.DesertColor, sphereProperties.DrylandsColor, sphereProperties.GrasslandsColor, sphereProperties.TundraColor, sphereProperties.FrozenDesertColor,
                    sphereProperties.DryForestColor, sphereProperties.ForestColor, sphereProperties.RainForestColor, sphereProperties.ColdForestColor, 
                    sphereProperties.desertHillsColor, sphereProperties.SmallHillsColor, sphereProperties.MesaColor, sphereProperties.HillsColor, sphereProperties.tundraHillsColor, sphereProperties.frozenHillsColor, sphereProperties.LargeHillsColor, sphereProperties.MountainsColor, sphereProperties.WarmOceanColor,
                    sphereProperties.ColdOceanColor, sphereProperties.GlacialColor
                };
            }

            public void GenerateVColors(Voronoi.Cell[] cells, Plate[] plates) {
                Color[][] vCells = new Color[cells.Length][];
                Color[][] vTectonicPlates = new Color[cells.Length][];
                Color[][] vHeight = new Color[cells.Length][];
                Color[][] vEvaporation = new Color[cells.Length][];
                Color[][] vTemperature = new Color[cells.Length][];
                Color[][] vHumidity = new Color[cells.Length][];
                Color[][] vBiome = new Color[cells.Length][];
                Color[][] vAboveWater = new Color[cells.Length][];

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
                    vEvaporation[a] = new Color[cells[a].TVertices.Length];
                    vTemperature[a] = new Color[cells[a].TVertices.Length];
                    vHumidity[a] = new Color[cells[a].TVertices.Length];
                    vBiome[a] = new Color[cells[a].TVertices.Length];
                    vAboveWater[a] = new Color[cells[a].TVertices.Length];
                    
                    Color cellColor = Random.ColorHSV();
                    Color heightColor = sphereProperties.HeightGradient.Evaluate(Data.GetScaledHeight(cells[a].Properties.Height));
                    Color evaporationColor = sphereProperties.EvaporationGradient.Evaluate(cells[a].Properties.Evaporation);
                    Color temperatureColor = sphereProperties.TemperatureGradient.Evaluate(Mathf.Lerp(Data.GetScaledTemperature(cells[a].Properties.Temperature[0]), Data.GetScaledTemperature(cells[a].Properties.Temperature[1]), sphereProperties.SimulatedTime));
                    Color humidityColor = sphereProperties.HumidityGradient.Evaluate(cells[a].Properties.Humidity);
                    Color biomeColor = biomeColors[(int)cells[a].Properties.Biome];

                    for(int b = 0; b < cells[a].TVertices.Length; b++) {
                        vCells[a][b] = cellColor;
                        
                        // UnityEngine.Debug.Log(cells[a].Plate.Type);
                        vTectonicPlates[a][b] = cells[a].Plate.Type == Plate.PlateType.continental ? 
                            landPlateColors[cells[a].Plate.Index] :
                            waterPlateColors[cells[a].Plate.Index];

                        vHeight[a][b] = heightColor;
                        vEvaporation[a][b] = evaporationColor;
                        vTemperature[a][b] = temperatureColor;
                        vHumidity[a][b] = humidityColor;
                        vBiome[a][b] = biomeColor;

                        vAboveWater[a][b] = Data.GetScaledHeight(cells[a].Properties.Height) > 0.5f ? heightColor : Color.black;
                    }
                }

                VCells = vCells;
                VTectonicPlates = vTectonicPlates;
                VHeight = vHeight;
                VEvaporation = vEvaporation;
                VTemperature = vTemperature;
                VHumidity = vHumidity;
                VBiome = vBiome;
                VAboveWater = vAboveWater;
            }

            public void ApplyVColor(Voronoi.Cell[] cells) {
                VoronoiMeshType v = sphereProperties.CurrentDisplayedVoronoi;
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
                        case VoronoiMeshType.evaporation:
                            cells[a].MeshRef.colors = VEvaporation[a];
                            break;
                        case VoronoiMeshType.temperature:
                            cells[a].MeshRef.colors = VTemperature[a];
                            break;
                        case VoronoiMeshType.humidity:
                            cells[a].MeshRef.colors = VHumidity[a];
                            break;
                        case VoronoiMeshType.biome:
                            cells[a].MeshRef.colors = VBiome[a];
                            break;
                        case VoronoiMeshType.aboveWater:
                            cells[a].MeshRef.colors = VAboveWater[a];
                            break;
                        default:
                            UnityEngine.Debug.LogError("ApplyVColor: VoronoiMeshType not in enum.");
                            break;
                    }
                }
            }

            public void AdjustVColor(Voronoi.Cell[] cells) {
                VoronoiMeshType v = sphereProperties.CurrentDisplayedVoronoi;
                UnityEngine.Debug.Log("Running");
                for(int a = 0; a < cells.Length; a++) {
                    switch (v) {
                        case VoronoiMeshType.temperature:
                            Color color = sphereProperties.TemperatureGradient.Evaluate(Mathf.Lerp(Data.GetScaledTemperature(cells[a].Properties.Temperature[0]), Data.GetScaledTemperature(cells[a].Properties.Temperature[1]), sphereProperties.SimulatedTime));
                            for(int b = 0; b < cells[a].TVertices.Length; b++) {
                                sphereProperties.Colors.VTemperature[a][b] = color;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
