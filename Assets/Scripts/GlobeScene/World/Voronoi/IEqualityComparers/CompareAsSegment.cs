using System.Collections.Generic;

namespace Generation.Voronoi
{
    public sealed class CompareAsSegment : IEqualityComparer<EdgeInt>
    {
        public bool Equals(EdgeInt edgeOne, EdgeInt edgeTwo)
        {
            return (edgeOne.A == edgeTwo.A && edgeOne.B == edgeTwo.B) || (edgeOne.A == edgeTwo.B && edgeOne.B == edgeTwo.B);
        }

        public int GetHashCode(EdgeInt edge)
        {

            int result = 121;
            result *= edge.A;
            result *= edge.B;
            return result;
        }
    }
}