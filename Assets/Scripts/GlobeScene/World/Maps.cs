using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using IDebug;
using static Noise.Simplex.Noise;

using static Generation.Data;
using static Generation.Voronoi.Plate;


namespace Generation.Voronoi {
    public static class Maps {
        public static void GeneratePlates(Sphere sphere, int plateNumber, float oToCRatio, int pContinentNumber, bool logMaps) {        
            TimeTracker timeTracker = null;
            if (logMaps) {
                timeTracker = new TimeTracker();
            }

            if (timeTracker != null) timeTracker.LogTimeStart("GeneratePlates()");
            
            Cell[] cells = sphere.Cells;
            HashSet<int> visited = new HashSet<int>();
            
            // Find seed index
            while (visited.Count < plateNumber) {
                visited.Add(Random.Range(0, cells.Length));
            }

            // Convert to array
            int[] init = visited.ToArray();

            List<int>[] ps = new List<int>[plateNumber];
            int[] place = new int[plateNumber];
            
            // Initialize
            for(int a = 0; a < plateNumber; a++) {
                ps[a] = new List<int>();
                place[a] = 0;
                ps[a].Add(init[a]);
            }

            System.Random rng = new System.Random();

            // Random flood fill
            while (visited.Count < sphere.Cells.Length) {
                
                
                int randomIndex = Random.Range(0,plateNumber);
                while (place[randomIndex] >= ps[randomIndex].Count) {
                    randomIndex = Random.Range(0,plateNumber);
                }
                
                // UnityEngine.Debug.Log($"{ps[randomIndex].Count}, {place[randomIndex]}");
                
                Cell currentCell = sphere.Cells[ps[randomIndex][place[randomIndex]]];
                int[] rni = Enumerable.Range(0, currentCell.Neighbors.Length).OrderBy((x) => rng.Next()).ToArray();

                for(int a = 0; a < currentCell.Neighbors.Length; a++) {
                    if (!visited.Contains(currentCell.Neighbors[rni[a]].Index)) {
                        visited.Add(currentCell.Neighbors[rni[a]].Index);

                        ps[randomIndex].Add(currentCell.Neighbors[rni[a]].Index);
                    }
                }

                place[randomIndex]++;
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Flood Fill Completed");
            // UnityEngine.Debug.Log(visited.Count);
            // UnityEngine.Debug.Log(cnt);
            
            int index = 0;
            Cell[] plateCells;
            Plate[] plates = new Plate[plateNumber];



            for(int a = 0; a < plateNumber; a++) {
                plateCells = new Cell[ps[a].Count];
                for(int b = 0; b < ps[a].Count; b++) {
                    plateCells[b] = sphere.Cells[ps[a][b]];
                }

                PlateType plateType = PlateType.oceanic;

                plates[a] = new Plate(plateCells, index, plateType);
                index++;
            }

            

            if (timeTracker != null) timeTracker.LogTimeElapsed("Plates Assigned");

            foreach(Plate plate in plates) {
                int[] pNeighbors = plate.FindBorderCells();
                Plate[] ns = new Plate[pNeighbors.Length];
                for(int a = 0; a < pNeighbors.Length; a++) {
                    ns[a] = plates[pNeighbors[a]];
                }
                plate.Neighbors = ns;
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Border Cells Found");
            
            
            place = new int[pContinentNumber];
            Plate[] continentCenters = new Plate[pContinentNumber];
            visited = new HashSet<int>();
            while(visited.Count < pContinentNumber) {
                visited.Add(Random.Range(0,pContinentNumber));
            }

            init = visited.ToArray();
            // With continentCenters, randomly assign their neighbors to an array of plates to become continental
            //  until you reach / surpass O to C ratio
            List<int>[] continents = new List<int>[pContinentNumber];
            for(int a = 0; a < continents.Length; a++)  {
                continents[a] = new List<int>();
                place[a] = 0;
                continents[a].Add(init[a]);
            }

            int cPlateNum = continentCenters.Length;
            float ratio = 1f - (float)cPlateNum / (float)plates.Length;
            while (ratio > oToCRatio) {
                
                int randomIndex = Random.Range(0,pContinentNumber);
                while (place[randomIndex] >= ps[randomIndex].Count) {
                    randomIndex = Random.Range(0,pContinentNumber);
                }

                Plate currentPlate = plates[continents[randomIndex][place[randomIndex]]];
                int[] rni = Enumerable.Range(0, currentPlate.Neighbors.Length).OrderBy((x) => rng.Next()).ToArray();
                
                for(int a = 0; a < currentPlate.Neighbors.Length; a++) {
                    if (!visited.Contains(currentPlate.Neighbors[rni[a]].Index)) {
                        visited.Add(currentPlate.Neighbors[rni[a]].Index);

                        continents[randomIndex].Add(currentPlate.Neighbors[rni[a]].Index);
                        cPlateNum++;
                    }
                }

                place[randomIndex]++;
                ratio = 1f - (float)cPlateNum / (float)plates.Length;
            }

            for(int a = 0; a < continents.Length; a++) {
                for(int b = 0; b < continents[a].Count; b++) {
                    plates[continents[a][b]].Type = PlateType.continental;
                }
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Continents Generated");
            if (timeTracker != null) timeTracker.LogTimeEnd();

            sphere.Plates = plates;
        }

        public static void GenerateHeight(Sphere sphere, float cnr, float cnb, float onr, float onb, float bs, int bt, bool logMaps) {
            TimeTracker timeTracker = null;
            if (logMaps) { timeTracker = new TimeTracker(); }
            
            if (timeTracker != null) timeTracker.LogTimeStart("GenerateHeight()");

            Plate[] plates = sphere.Plates;

            // Find Heights
            foreach(Plate plate in plates) {

                HashSet<int> borderCells = new HashSet<int>();
                float avgHeight = 0f;
                float avgInd = 0f;
                foreach(Cell[] border in plate.Borders) {
                    
                    foreach(Cell cell in border) {
                        float height = Data.Heights[(int)cell.Plate.Type];
                        float ind = 1f;
                        foreach(Cell neighbor in cell.Neighbors) {
                            height += (1f + cell.Plate.Speed + -Vector2.Dot(cell.Plate.Direction, neighbor.Plate.Direction) * neighbor.Plate.Speed) * Heights[(int)neighbor.Plate.Type];
                            ind += 1f;
                        }
                        ApplyNoise(height / ind, cell);

                        avgHeight += height / ind;
                        avgInd += 1f;

                        borderCells.Add(cell.Index);                       
                    }
                }

                foreach(Cell cell in plate.Cells) {
                    if (!borderCells.Contains(cell.Index)) ApplyNoise(avgHeight/avgInd, cell);
                }
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Assigned Height");
            
            // Blend
            Cell[] cells = new Cell[0];
            foreach(Plate plate in plates) {
                cells = cells.Concat<Cell>(plate.Cells).ToArray();
            }
            for(int a = 0; a < bt; a++) {
                int start = Random.Range(0,cells.Length);
                foreach(Cell cell in cells) {
                    float divisor = 1f;

                    float height = cell.Properties.Height;
                    foreach(Cell neighbor in cell.Neighbors) {
                        height += neighbor.Properties.Height * bs;
                        divisor += bs;
                    }

                    cell.Properties.Height = height / divisor;
                }
            }

            // Reassign the material, continents get MapOutlined
            foreach(Transform child in sphere.MeshObject.transform.GetChild(1).transform) {
                if (Data.GetScaledHeight(sphere.Cells[int.Parse(Regex.Replace(child.name, @"[^\d]", ""))].Properties.Height) > 0.5f) {
                    child.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Globe/MapOutline");
                    child.gameObject.layer = 6;
                }
                else {
                    child.gameObject.layer = 7;
                }
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Blended Height");
            if (timeTracker != null) timeTracker.LogTimeEnd();

            void ApplyNoise(float height, Cell cell) {
                switch (cell.Plate.Type) {
                    case PlateType.continental:
                        cell.Properties.Height = height + height * cnr * (Random.value + cnb);
                        break;
                    case PlateType.oceanic:
                        cell.Properties.Height = height + height * onr * (Random.value + onb);
                        break;
                        default:
                        break;
                } 
            }
        }
        public static void GenerateEvaporation(Sphere sphere, Vector3 eSeed, float eFrequency) {
            // 0.5 - 1 , 0 =n/a, 0.5 -> -1, 0.75 -> 0, 1 -> 1
            foreach(Cell cell in sphere.Cells) {
                cell.Properties.Evaporation = GetScaledHeight(cell.Properties.Height) > 0.5f ? (Simplex3D(cell.Center + eSeed, eFrequency).value + 1f) / 4f + 0.25f : 0f;
            }
        }
        
        public static void GenerateTemperature(Sphere sphere, float lBuffer, float hBuffer, float cnr, float cnb, float onr, float onb) {
            Cell[] cells = sphere.Cells;
            
            // Bounds are a unit sphere (-1, 1)
            // temperature range is -30 C to 40 C
            // over the course of a year, temperature changes between two levels
            // higher altitudes are more sensitive to changes, and colder
            // higher latitudes are colder

            // Temperature decrease per every 100 meters height
            float hm = -0.7f;
            // temperature decrease per every 1 degree latitude 
            float tm = -1.5f + lBuffer;
            // Winter temp mod assigned in humidity
            foreach (Cell cell in cells) {
                float heightMod = GetScaledHeight(cell.Properties.Height) > hBuffer ? hm * (cell.Properties.Height - GetTrueHeight(hBuffer)) / 750f : 0f;
                float latMod = Mathf.Abs(cell.Center.y) > lBuffer ? tm * 37f * (Mathf.Abs(cell.Center.y) - lBuffer) : 0f;
                cell.Properties.Temperature[0] = cell.Plate.Type == PlateType.continental ? ApplyNoise(37f + heightMod + latMod) : ApplyNoise(Mathf.Clamp(37f + heightMod + latMod, -30f, 30f)); 
            }

            float ApplyNoise(float val) {
                return val;
            }
        }

        public static void GenerateHumidity(Sphere sphere) {
            foreach(Cell cell in sphere.Cells) {
                // 0 - 1
                // Humidity based on temperature and evaporation
                float tempMod = Mathf.Clamp((GetScaledTemperature(cell.Properties.Temperature[0]) + 0.75f) / 1.5f, 0.5f, 1f);
                float evapMod = cell.Properties.Evaporation > -1f ? (cell.Properties.Evaporation -0.33f) / 0.33f * tempMod : 0f;
                cell.Properties.Humidity = evapMod;

                // Colder temp based on height and humidity
                cell.Properties.Temperature[1] = cell.Properties.Temperature[0];
            }
        }

        public static void GenerateBiome(Sphere sphere) {
            foreach(Cell cell in sphere.Cells) {
                Cell.GenerationValues p = cell.Properties;
                float hei = GetScaledHeight(p.Height);
                float tmp = GetScaledTemperature(p.Temperature[0]);
                float hmd = p.Humidity;

                // Height
                bool flat = hei > 0.5f && hei <= 0.65f;
                bool sHill = hei > 0.65f && hei <= 0.75f;
                bool hill = hei > 0.75f && hei <= 0.8f;
                bool lHill = hei > 0.8f && hei <= 0.85f;
                bool moun = hei > 0.85f;

                // Temperature
                bool hot = tmp > 0.9f;
                bool warm = tmp > 0.7f && tmp <= 0.9f;
                bool temp = tmp > 0.5f && tmp <= 0.7f;
                bool cool = tmp > 0.33f && tmp <= 0.5f;
                bool cold = tmp <= 0.33f;

                // Humidity
                bool vHum = hmd > 0.85f;
                bool hum = hmd > 0.6f && hmd <= 0.85f;
                bool mild = hmd > 0.4f && hmd <= 0.6f;
                bool arid = hmd > 0.15f && hmd <= 0.4f;
                bool vArid = hmd <= 0.15f;
                if (moun) {
                    cell.Properties.Biome = BiomeType.mountains;
                }
                else if (lHill) {
                    cell.Properties.Biome = BiomeType.largeHills;
                }
                else if (hill) {
                    if (vArid && hot) {
                        cell.Properties.Biome = BiomeType.mesa;
                    }
                    else {
                        cell.Properties.Biome = BiomeType.hills;
                    }   
                    
                }
                else if (sHill) {
                    cell.Properties.Biome = BiomeType.smallHills;
                }
                else if (flat) {
                    if (vArid) {
                        if (cold) {
                            cell.Properties.Biome = BiomeType.frozenDesert;
                        }
                        else if (cool) {
                            cell.Properties.Biome = BiomeType.tundra;
                        }
                        else if (temp) {
                            cell.Properties.Biome = BiomeType.drylands;
                        }
                        else {
                            cell.Properties.Biome = BiomeType.desert;
                        }
                    }   
                    else {
                        cell.Properties.Biome = BiomeType.grasslands;
                    }
                }
                else {
                    if (cold) {
                        cell.Properties.Biome = BiomeType.glacial;
                    }
                    else if (cool || temp) {
                        cell.Properties.Biome = BiomeType.coldOcean;
                    }
                    else {
                        cell.Properties.Biome = BiomeType.warmOcean;
                    }
                }
            }
        }
        public static void GenerateTerrain(Sphere sphere) {

        }
    }
}