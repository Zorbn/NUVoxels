using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    public const int MapWidthInChunks = 12;
    public const int MapHeightInChunks = 8;
    public const float GroundLevel = ChunkRenderer.Size * MapHeightInChunks * 0.5f;
    
    // TODO List:
    // Consider splitting chunk renderer into chunk renderer and chunk data classes
    
    [SerializeField] private GameObject chunkPrefab;
    
    private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private Vector2[] textureOffsets;
    private Material atlasMaterial;
    private Texture2DArray texArray;

    private readonly Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new();
    private readonly ConcurrentQueue<ChunkRenderer> chunkRenderersToUpdate = new();

    private void Start()
    {
        texArray = new Texture2DArray(CubeMesh.CubeTexWidth, CubeMesh.CubeTexHeight, 
            Block.BlockCount * Block.SideCount, TextureFormat.ARGB32, 3, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        FillTextureArray();

        atlasMaterial = new Material(Shader.Find("Custom/StandardTextureArray"));
        atlasMaterial.SetTexture(MainTex, texArray);
        atlasMaterial.SetFloat(Glossiness, 0);

        InitializeChunks();
        GenerateTerrain();

        Thread meshGenThread = new(MeshGenThreadProc);
        meshGenThread.Start();
    }

    private void GenerateTerrain()
    {
        Parallel.ForEach(chunkRenderers, pair => { pair.Value.GenerateData(); });
    }

    private void InitializeChunks()
    {
        for (int i = 0; i < MapWidthInChunks * MapWidthInChunks * MapHeightInChunks; i++)
        {
            (int x, int y, int z) = i.To3D(MapWidthInChunks, MapHeightInChunks);
            InstantiateChunk(new Vector3Int(x, y, z));
        }
    }

    private void FillTextureArray()
    {
        for (int bi = 0; bi < Block.BlockCount; bi++)
        {
            Block.Id blockId = (Block.Id)bi;
            string blockName = blockId.ToString();

            for (int si = 0; si < Block.SideCount; si++)
            {
                Direction.Axis sideId = (Direction.Axis)si;
                string sideName = sideId.ToString();
                Texture2D texture2D = Resources.Load<Texture2D>($"Blocks/{blockName}/{sideName}");

                if (texture2D == null)
                    throw new FileLoadException($"Failed to load texture from 'Blocks/{blockName}/{sideName}'");

                texArray.SetPixels(texture2D.GetPixels(), Block.GetTextureId(blockId, sideId));
            }
        }

        texArray.Apply();
    }

    private void Update()
    {
        while (chunkRenderersToUpdate.TryDequeue(out ChunkRenderer queuedRenderer)) queuedRenderer.UpdateMesh();
    }

    private void MeshGenThreadProc()
    {
        while (true)
        {
            foreach (KeyValuePair<Vector3Int, ChunkRenderer> pair in chunkRenderers)
            {
                if (!pair.Value.IsDirty) continue;
                
                pair.Value.GenerateMeshData();
                chunkRenderersToUpdate.Enqueue(pair.Value);
            }
        }
    }

    public Block.Id GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);
    
    public Block.Id GetBlock(int x, int y, int z)
    {
        Vector3Int chunkPos = GetChunkPos(x, y, z);
        if (!chunkRenderers.ContainsKey(chunkPos)) return Block.Id.Air;

        Vector3Int localPos = GetLocalPos(x, y, z);
        return chunkRenderers[chunkPos].GetBlock(localPos);
    }

    public void SetBlock(Block.Id blockId, Vector3Int pos) => SetBlock(blockId, pos.x, pos.y, pos.z);
    
    public void SetBlock(Block.Id blockId, int x, int y, int z)
    {
        Vector3Int chunkPos = GetChunkPos(x, y, z);
        if (!chunkRenderers.ContainsKey(chunkPos)) return;

        Vector3Int localPos = GetLocalPos(x, y, z);
        ChunkRenderer chunkRenderer = chunkRenderers[chunkPos];
        chunkRenderer.SetBlock(blockId, localPos);
        
        if (localPos.x == 0)                      MarkDirty(chunkPos + Vector3Int.left);
        if (localPos.x == ChunkRenderer.Size - 1) MarkDirty(chunkPos + Vector3Int.right);
        if (localPos.y == 0)                      MarkDirty(chunkPos + Vector3Int.down);
        if (localPos.y == ChunkRenderer.Size - 1) MarkDirty(chunkPos + Vector3Int.up);
        if (localPos.z == 0)                      MarkDirty(chunkPos + Vector3Int.back);
        if (localPos.z == ChunkRenderer.Size - 1) MarkDirty(chunkPos + Vector3Int.forward);
    }

    // Get the position of the chunk containing this block.
    private static Vector3Int GetChunkPos(int x, int y, int z) => new()
    {
        x = x / ChunkRenderer.Size,
        y = y / ChunkRenderer.Size,
        z = z / ChunkRenderer.Size
    };
    
    // Get the location of this block inside the chunk containing it.
    private static Vector3Int GetLocalPos(int x, int y, int z) => new()
    {
        x = x % ChunkRenderer.Size,
        y = y % ChunkRenderer.Size,
        z = z % ChunkRenderer.Size
    };

    private void MarkDirty(Vector3Int chunkPos)
    {
        if (chunkRenderers.ContainsKey(chunkPos)) chunkRenderers[chunkPos].MarkDirty();
    }
    
    private void InstantiateChunk(Vector3Int position)
    {
        float worldX = position.x * ChunkRenderer.Size,
              worldY = position.y * ChunkRenderer.Size,
              worldZ = position.z * ChunkRenderer.Size;
        
        GameObject chunk = Instantiate(chunkPrefab, new Vector3(worldX, worldY, worldZ), Quaternion.identity);
        ChunkRenderer chunkRenderer = chunk.GetComponent<ChunkRenderer>();
        chunkRenderer.material = atlasMaterial;
        chunkRenderer.world = this;
        chunkRenderer.chunkPosition = position;
        
        chunkRenderers.Add(position, chunkRenderer);
    }
}
