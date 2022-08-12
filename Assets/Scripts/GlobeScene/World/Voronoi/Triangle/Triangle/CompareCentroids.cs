using System.Collections.Generic;

using Generation.Voronoi;
    public sealed class CompareCentroids : IEqualityComparer<Triangle>
    {
        public bool Equals(Triangle t1, Triangle t2)
        {
            return t1.Centroid == t2.Centroid;
        }

        public int GetHashCode(Triangle t)
        {
            return t.Centroid.GetHashCode();
        }
    }

