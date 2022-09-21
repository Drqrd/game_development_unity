using UnityEngine;
using System.Collections.Generic;

namespace Generation.Voronoi {
    public class Plate
    {
        public Cell[] Cells { get; private set; }
        public Cell[][] Borders { get; private set; }
        public Plate[] Neighbors { get; private set; }
        public int Index { get; private set; }
        public PlateType Type { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }
        public Color[] debugColors { get; private set; }

        public enum PlateType {
            continental,
            oceanic,
        }

        public Plate(Cell[] cells, int index, float oToCRatio) {
            Plate[] neighbors = new Plate[0];
            Cells = cells;
            Neighbors = neighbors;
            Index = index;

            // UnityEngine.Debug.Log(index);

            foreach(Cell cell in cells) {
                cell.Plate = this;
            }

            Type = Random.value <= oToCRatio ? PlateType.oceanic : PlateType.continental;
        
            Direction = new Vector2(Random.value,Random.value);
            Speed = Random.value / 1.25f;
        }

        public void FindBorderCells() {
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
        }
    }
}
