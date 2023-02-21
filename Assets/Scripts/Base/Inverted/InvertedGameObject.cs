using UnityEngine;

public class InvertedGameObject : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private Material material = null;
    [SerializeField] [Range(0.1f, 5000f)] private float size;

    public void Generate() {
        GameObject self = this.gameObject;

        if (self.TryGetComponent<MeshFilter>(out MeshFilter mf)) DestroyImmediate(mf);
        if (self.TryGetComponent<MeshRenderer>(out MeshRenderer mr)) DestroyImmediate(mr);

        Mesh mesh = target.GetComponent<MeshFilter>().sharedMesh;

        int[] ts = mesh.triangles;
        for(int a = 0; a < ts.Length; a += 3){
            int save = ts[a];
            ts[a] = ts[a + 2];
            ts[a + 2] = save;
        }

        mesh.triangles = ts;
        mesh.RecalculateNormals();

        MeshRenderer meshRenderer = self.AddComponent<MeshRenderer>();
        if (material != null) meshRenderer.material = material;
        MeshFilter meshFilter = self.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        transform.localScale = new Vector3(size,size,size);
    }
}
