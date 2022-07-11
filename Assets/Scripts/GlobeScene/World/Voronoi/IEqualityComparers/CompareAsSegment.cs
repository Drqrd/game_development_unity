using UnityEngine;
using System.Collections.Generic;

namespace Generation.Voronoi
{

    public sealed class CompareAsSegment : IEqualityComparer<Edge>
    {
        public bool Equals(Edge edgeOne, Edge edgeTwo)
        {
            return edgeOne.Midpoint == edgeTwo.Midpoint;
        }

        public int GetHashCode(Edge edge)
        {

            return edge.Midpoint.GetHashCode();
        }
    }
}