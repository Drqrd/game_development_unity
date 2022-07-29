using System.Collections.Generic;
using UnityEngine;

namespace Generation.Voronoi
{
    public class DebugTriangle : MonoBehaviour
    {
        public List<Triangle> Neighbors { get; set; }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                foreach(Triangle n in Neighbors)
                {
                    Gizmos.DrawSphere(n.Centroid, 0.05f);
                }
            }
        }
    }
}
