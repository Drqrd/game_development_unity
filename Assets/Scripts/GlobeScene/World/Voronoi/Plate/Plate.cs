using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Generation.Voronoi {
    public class Plate
    {
        public Cell[] Cells { get; private set; }
        public Cell[][] Borders { get; private set; }
        public Plate[] Neighbors { get; set; }
        public int Index { get; private set; }
        public PlateType Type { get; set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }
        public Color[] debugColors { get; private set; }

        public enum PlateType {
            continental,
            oceanic
        }

        public Plate(Cell[] cells, int index, PlateType type) {
            Cells = cells;
            Index = index;
            Type = type;

            // UnityEngine.Debug.Log(index);

            foreach(Cell cell in cells) {
                cell.Plate = this;
            }
        
            Direction = new Vector2(Random.value,Random.value);
            Speed = Random.value / 1.25f;
        }

        public int[] FindBorderCells() {
            Dictionary<int, List<Cell>> cellDict = new Dictionary<int, List<Cell>>();
            foreach(Cell cell in Cells) {
                foreach(Cell neighbor in cell.Neighbors) {
                    // UnityEngine.Debug.Log(neighbor.Plate.Index);
                    // UnityEngine.Debug.Log(cell.Plate.Index);
                    if (neighbor.Plate.Index != cell.Plate.Index) {
                        if (!cellDict.ContainsKey(neighbor.Plate.Index)) {
                            cellDict.Add(neighbor.Plate.Index, new List<Cell>());
                        }
                        cellDict[neighbor.Plate.Index].Add(cell);
                    }
                }
            }

            Cell[][] borderCells = new Cell[cellDict.Count][];
            int index = 0;
            foreach(KeyValuePair<int,List<Cell>> kvp in cellDict) {
                borderCells[index] = kvp.Value.ToArray();
                index++;
            }

            Borders = borderCells;
            debugColors = new Color[borderCells.Length];
            for(int a = 0; a < debugColors.Length; a++) {
                debugColors[a] = Random.ColorHSV();
            }

            return cellDict.Keys.ToArray();
        }
    }
}
