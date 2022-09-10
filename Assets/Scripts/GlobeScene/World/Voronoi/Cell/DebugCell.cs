using UnityEngine;

namespace Generation.Voronoi {
    public class DebugCell : MonoBehaviour
    {
        // Start is called before the first frame update
        public Vector3[] Vertices { get; set; }
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                for(int b = 1; b < Vertices.Length;b++) {
                    Gizmos.DrawLine(Vertices[b], Vertices[b-1]);
                }
                Gizmos.DrawLine(Vertices[Vertices.Length - 1], Vertices[0]);
            }
        }
    }
}   
