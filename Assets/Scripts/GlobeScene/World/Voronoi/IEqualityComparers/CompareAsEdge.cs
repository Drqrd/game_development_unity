using System.Collections.Generic;

namespace Generation.Voronoi
{
    public sealed class CompareAsEdge : IEqualityComparer<EdgeInt>
    {
        public bool Equals(EdgeInt edgeOne, EdgeInt edgeTwo)
        {
            int AA, AB, BA, BB;
            if (edgeOne.A > edgeOne.B) 
            {
                AA = edgeOne.A;
                AB = edgeOne.B;
            }
            else
            {
                AA = edgeOne.B;
                AB = edgeOne.A;
            }
            if (edgeTwo.A > edgeTwo.B)
            {
                BA = edgeTwo.A;
                BB = edgeTwo.B;
            }
            else
            {
                BA = edgeTwo.B;
                BB = edgeTwo.A;
            }
            return AA == BA && AB == BB;
        }

        public int GetHashCode(EdgeInt edge)
        {

            int result = 12;
            result *= 17 * edge.A;
            result *= edge.B;
            return result;
        }
    }
}