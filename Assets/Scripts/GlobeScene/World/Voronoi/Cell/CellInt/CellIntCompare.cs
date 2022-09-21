using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation.Voronoi {
    public sealed class CellIntCompareSegments : IEqualityComparer<CellInt>
    {
        public bool Equals(CellInt cellOne, CellInt cellTwo)
        {
            List<int> pOne = cellOne.Points.ConvertAll(x => x), pTwo = cellTwo.Points.ConvertAll(x => x);

            pOne.Sort();
            pTwo.Sort();
            
            return pOne.SequenceEqual(pTwo);
        }

        public int GetHashCode(CellInt cell)
        {

            unchecked {
                List<int> points = cell.Points.ConvertAll(x => x);
                points.Sort();

                IHelper.List<int>.Log(points);
                UnityEngine.Debug.Log("-----");

                int hash = 19;
                foreach(int point in points) {
                    hash *= 31 + point.GetHashCode();
                }
                return hash;
            }
        }
    }
}
