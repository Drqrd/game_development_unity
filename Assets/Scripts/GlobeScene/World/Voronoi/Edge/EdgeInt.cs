using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Voronoi
{
    public class EdgeInt : BaseObject
    {
        public int A { get; private set; }
        public int B { get; private set; }

        public EdgeInt(int A, int B)
        {
            this.A = A;
            this.B = B;
        }
    }
}
