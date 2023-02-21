using UnityEngine;

namespace Generation.Voronoi.Debug {
    public class Cell : MonoBehaviour
    {
        // Start is called before the first frame update
        public Voronoi.Cell cell { get; set; }
        private MeshRenderer meshRenderer;
        private Camera cam;
        private GlobeCameraController camController;
        private void Start() {
            meshRenderer = GetComponent<MeshRenderer>();
            cam = Camera.main;
            camController = cam.GetComponent<GlobeCameraController>();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                /*
                Gizmos.color = Color.red;
                for(int b = 0; b < cell.Neighbors.Length;b++) {
                    Gizmos.DrawSphere(cell.Neighbors[b].Center, 0.01f);
                }
                */

                Gizmos.DrawWireCube(transform.position, meshRenderer.bounds.size);
            }
        }
    }
}   
