using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Voronoi {
    public class CellInt
    {
        public List<int> Points {get; private set; }

        public CellInt (List<int> points){
            this.Points = points;
        }
    }
}

