using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation.Voronoi {
    public sealed class CellIntCompareSegments : IEqualityComparer<CellInt>
    {
        public bool Equals(CellInt cellOne, CellInt cellTwo)
        {
            if (cellOne.Points.Count != cellTwo.Points.Count) return false;

            List<int> cellPointsOne = new List<int>(cellOne.Points.Count), cellPointsTwo = new List<int>(cellTwo.Points.Count);
            cellPointsOne.AddRange(cellOne.Points.Select(x => x));
            cellPointsTwo.AddRange(cellTwo.Points.Select(x => x));

            cellPointsOne.Sort();
            cellPointsTwo.Sort();

            for(int a = 0; a < cellPointsOne.Count; a++) if (cellPointsOne[a] != cellPointsTwo[a]) return false;

            return true;
        }

        public int GetHashCode(CellInt cell)
        {
            int mod = cell.Points.Count * 17, result = 12;

            foreach(int i in cell.Points) {
                result *= mod * i;
                mod -= 17;
            }

            return result;
        }
    }
}
