using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using IDebug;
using LineFunctions;
using static Generation.Data;

namespace Generation.Voronoi {
    public static class Maps {
        public static void GeneratePlates(Sphere sphere, int plateNumber, float oToCRatio, bool logMaps) {        
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
            int cnt = 0;
            while (visited.Count < sphere.Cells.Length && cnt < 10000) {
                
                
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
                cnt++;
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
                plates[a] = new Plate(plateCells, index, oToCRatio);
                index++;
            }

            sphere.Plates = plates;

            if (timeTracker != null) timeTracker.LogTimeElapsed("Plates Assigned");

            foreach(Plate plate in sphere.Plates) {
                plate.FindBorderCells();
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Border Cells Found");
            if (timeTracker != null) timeTracker.LogTimeEnd();
        }

        public static void GenerateHeight(Sphere sphere, float cnr, float cnb, float onr, float onb, int bl, float bs, int bt, bool logMaps) {
            TimeTracker timeTracker = null;
            if (logMaps) { timeTracker = new TimeTracker(); }
            
            if (timeTracker != null) timeTracker.LogTimeStart("GenerateHeight()");

            Plate[] plates = sphere.Plates;

            // Find Heights
            foreach(Plate plate in plates) {
                foreach(Cell cell in plate.Cells) {
                    ApplyNoise(Heights[(int)cell.Plate.Type], cell);
                }

                foreach(Cell[] border in plate.Borders) {
                    foreach(Cell cell in border) {
                        float height = Heights[(int)cell.Plate.Type];
                        foreach(Cell neighbor in cell.Neighbors) {
                            height += (cell.Plate.Speed + Vector2.Dot(cell.Plate.Direction, neighbor.Plate.Direction) * neighbor.Plate.Speed) * Heights[(int)neighbor.Plate.Type];
                        }
                        ApplyNoise(height / (cell.Neighbors.Length + 1), cell);                        
                    }
                }
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Assigned Height");

            if (timeTracker != null)
            // Blend
            foreach(Plate plate in plates) {
                foreach(Cell[] border in plate.Borders) {
                    HashSet<int> borderCellIndices = new HashSet<int>();
                    for(int a = 0; a < border.Length; a++) {
                        borderCellIndices.Add(border[a].Index);
                    }
                    foreach(Cell cell in border) {
                        for (int a = 0; a < bt; a++) { Blend(cell, borderCellIndices); }
                    }
                }
            }

            if (timeTracker != null) timeTracker.LogTimeElapsed("Blended Height");
            if (timeTracker != null) timeTracker.LogTimeEnd();

            void Blend(Cell cell, HashSet<int> exclude, int rl = 1) {
                float divisor = 1f;
                float height = cell.Properties.Height;
                foreach(Cell neighbor in cell.Neighbors) {
                    if (!exclude.Contains(neighbor.Index)) {
                        height += neighbor.Properties.Height * bs;
                        divisor += bs;
                    }
                }

                exclude.Add(cell.Index);

                cell.Properties.Height = height / divisor;

                foreach(Cell neighbor in cell.Neighbors) {
                    if (!exclude.Contains(neighbor.Index) && 
                        rl < bl) {
                        Blend(neighbor, exclude, rl + 1);
                    }
                }
            }

            void ApplyNoise(float height, Cell cell) {
                if (cell.Plate.Type == Plate.PlateType.continental) {
                    cell.Properties.Height = height + height * cnr * (Random.value + cnb);
                }  
                else {
                    cell.Properties.Height = height + height * onr * (Random.value + onb);
                }
            }
        }
        
        public static void GenerateTemperature(Sphere sphere) {

        }

        public static void GenereateHumidity(Sphere sphere) {

        }

        public static void GenerateBiome(Sphere sphere) {
            
        }
        public static void GenerateTerrain(Sphere sphere) {

        }
    }
}