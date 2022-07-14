using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    public const int Size = 32;

    public bool IsDirty { get; private set; }

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    
    public World world;
    public Vector3Int chunkPosition;
    public Material material;

    private readonly Block.Id[] blocks = new Block.Id[Size * Size * Size];
    
    private readonly List<Vector3> vertices = new();
    private readonly List<int> indices = new();
    private readonly List<Vector3> uv = new();
    private int indexOffset;
    private Mesh mesh;

    // TODO: Make this function take an Action that handles the actual terrain gen?
    public void GenerateData()
    {
        Vector3Int chunkWorldPos = chunkPosition * Size;
        
        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                int worldX = chunkWorldPos.x + x,
                    worldZ = chunkWorldPos.z + z;
                
                float noise = Mathf.PerlinNoise(worldX / 100f, worldZ / 100f);
                int maxHeight = (int)(noise * World.GroundLevel - chunkWorldPos.y);
                
                for (int y = 0; y < maxHeight; y++)
                {
                    // int worldY = chunkWorldPos.y + y;
                    Block.Id blockId = (Block.Id)SharedRandom.Default.Next(1, 3);
                    SetBlock(blockId, x, y, z);
                }
            }
        }
    }

    public void GenerateMeshData()
    {
        vertices.Clear();
        indices.Clear();
        uv.Clear();
        indexOffset = 0;

        for (int i = 0; i < Size * Size * Size; i++)
        {
            (int x, int y, int z) = i.To3D(Size);

            Block.Id id = GetBlock(x, y, z);
            if (id != Block.Id.Air) GenCube(id, x, y, z);
        }

        IsDirty = false;
    }
    
    public void UpdateMesh()
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
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.SetUVs(0, uv);
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    private void GenCube(Block.Id blockId, int x, int y, int z)
    {
        for (int i = 0; i < 6; i++) GenFaceIfNecessary(blockId, x, y, z, (Direction.Axis)i);
    }

    public Block.Id GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);
    
    public Block.Id GetBlock(int x, int y, int z) =>
        IsBlockInBounds(x, y, z) ? blocks[(x, y, z).To1D(Size)] : Block.Id.Air;
    
    public void SetBlock(Block.Id blockId, Vector3Int pos) => SetBlock(blockId, pos.x, pos.y, pos.z);
    
    public void SetBlock(Block.Id blockId, int x, int y, int z)
    {
        if (!IsBlockInBounds(x, y, z)) return;

        blocks[(x, y, z).To1D(Size)] = blockId;
        IsDirty = true;
    }

    public void MarkDirty() => IsDirty = true;

    private static bool IsBlockInBounds(int x, int y, int z) => x is >= 0 and < Size &&
                                                                y is >= 0 and < Size &&
                                                                z is >= 0 and < Size;

    private void GenFaceIfNecessary(Block.Id blockId, int x, int y, int z, Direction.Axis direction)
    {
        Vector3 offset = Direction.AxisToVec(direction);
        Vector3Int pos = new(x, y, z);
        
        if (world.GetBlock(pos + Vector3Int.RoundToInt(offset) + chunkPosition * Size) == Block.Id.Air)
            GenFace(blockId, x, y, z, direction);
    }

    private void GenFace(Block.Id blockId, int x, int y, int z, Direction.Axis direction)
    {
        Vector3 position = new(x, y, z);
        int faceVerticesLength = CubeMesh.FaceVertices[direction].Length;
        
        for (int i = 0; i < faceVerticesLength; i++)
        {
            vertices.Add(position + CubeMesh.FaceVertices[direction][i]);
            
            Vector2 originalUv = CubeMesh.FaceUV[direction][i];
            Vector3 offsetUv = new(originalUv.x, originalUv.y, Block.GetTextureId(blockId, direction));

            uv.Add(offsetUv);
        }
        
        for (int i = 0; i < CubeMesh.FaceIndices[direction].Length; i++)
        {
            indices.Add(indexOffset + CubeMesh.FaceIndices[direction][i]);
        }

        indexOffset += faceVerticesLength;
    }
}