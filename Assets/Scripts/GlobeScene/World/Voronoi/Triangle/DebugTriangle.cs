using System.Collections.Generic;
using UnityEngine;

namespace Generation.Voronoi.Debug
{
    public class Triangle : MonoBehaviour
    {
        public List<Voronoi.Triangle> Neighbors { get; set; }

        private void OnDrawGizmosSelected()
        {
            /*
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                foreach(Triangle n in Neighbors)
                {
                    Gizmos.DrawSphere(n.Centroid, 0.01f);
                }
            }
            */
        }
    }
}
