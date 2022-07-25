using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Voronoi
{
    public class Centroid
    {
        public List<Centroid> Neighbors { get; set; }
        public Vector3 Self { get; private set; }

        public Centroid(Vector3 v)
        {
            Self = v;
        }
    }


}
