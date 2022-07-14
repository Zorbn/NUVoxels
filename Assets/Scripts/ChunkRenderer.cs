using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    [HideInInspector] public Material material;
    
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;
    
    private Mesh mesh;

    public void UpdateMesh(ChunkData data)
    {
        if (mesh is null)
        {
            mesh = new Mesh
            {
                // Allow mesh to store enough vertices to render a whole chunk,
                // the default of UInt16 is too low for this purpose
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            meshFilter.mesh = mesh;
            meshRenderer.material = material;
            meshCollider.sharedMesh = mesh;
        }

        mesh.Clear();
        mesh.SetVertices(data.vertices);
        mesh.SetTriangles(data.indices, 0);
        mesh.SetUVs(0, data.uv);
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }
}