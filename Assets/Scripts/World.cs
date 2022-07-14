using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// TODO: Prevent placing blocks if the location is occupied (check pos using unity physics)

public class World : MonoBehaviour
{
    private const int WidthInChunks = 12;
    private const int HeightInChunks = 8;
    private const float GroundLevel = Chunk.Size * HeightInChunks * 0.5f;
    
    [SerializeField] private GameObject chunkPrefab;

    private readonly Dictionary<Vector3Int, Chunk> chunks = new();
    private readonly ConcurrentQueue<Chunk> chunksToUpdate = new();

    private readonly VoxelTextureArray textureArray = new();

    private void Start()
    {
        textureArray.Initialize();
        
        InitializeChunks();
        GenerateTerrain((pos, random) =>
        {
            float noise = Mathf.PerlinNoise(pos.x / 100f, pos.z / 100f);
            int maxHeight = (int)(noise * GroundLevel);

            if (pos.y >= maxHeight) return;
            
            Block.Id blockId = (Block.Id)random.Next(1, 3);
            SetBlock(blockId, pos);
        });

        Thread meshGenThread = new(MeshGenThreadProc);
        meshGenThread.Start();
    }

    private void GenerateTerrain(Action<Vector3Int, System.Random> generator)
    {
        Parallel.ForEach(chunks, pair => { pair.Value.data.Generate(generator); });
    }

    private void InitializeChunks()
    {
        const int sizeInChunks = WidthInChunks * WidthInChunks * HeightInChunks;
        for (int i = 0; i < sizeInChunks; i++)
        {
            (int x, int y, int z) = i.To3D(WidthInChunks, HeightInChunks);
            InstantiateChunk(new Vector3Int(x, y, z));
        }
    }

    private void Update()
    {
        while (chunksToUpdate.TryDequeue(out Chunk chunk)) chunk.renderer.UpdateMesh(chunk.data);
    }

    private void MeshGenThreadProc()
    {
        List<Chunk> dirtyChunks = new();
        
        while (true)
        {
            foreach ((Vector3Int _, Chunk chunk) in chunks)
            {
                if (chunk.data.IsDirty) dirtyChunks.Add(chunk);
            }

            Parallel.ForEach(dirtyChunks, chunk =>
            {
                chunk.data.GenerateMesh();
                chunksToUpdate.Enqueue(chunk);
            });
            
            dirtyChunks.Clear();
        }
    }

    public Block.Id GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);
    
    public Block.Id GetBlock(int x, int y, int z)
    {
        Vector3Int chunkPos = GetChunkPos(x, y, z);
        if (!chunks.ContainsKey(chunkPos)) return Block.Id.Air;

        Vector3Int localPos = GetLocalPos(x, y, z);
        return chunks[chunkPos].data.GetBlock(localPos);
    }

    public void SetBlock(Block.Id blockId, Vector3Int pos) => SetBlock(blockId, pos.x, pos.y, pos.z);
    
    public void SetBlock(Block.Id blockId, int x, int y, int z)
    {
        Vector3Int chunkPos = GetChunkPos(x, y, z);
        if (!chunks.ContainsKey(chunkPos)) return;

        Vector3Int localPos = GetLocalPos(x, y, z);
        Chunk chunk = chunks[chunkPos];
        chunk.data.SetBlock(blockId, localPos);
        
        if (localPos.x == 0)              MarkDirty(chunkPos + Vector3Int.left);
        if (localPos.x == Chunk.Size - 1) MarkDirty(chunkPos + Vector3Int.right);
        if (localPos.y == 0)              MarkDirty(chunkPos + Vector3Int.down);
        if (localPos.y == Chunk.Size - 1) MarkDirty(chunkPos + Vector3Int.up);
        if (localPos.z == 0)              MarkDirty(chunkPos + Vector3Int.back);
        if (localPos.z == Chunk.Size - 1) MarkDirty(chunkPos + Vector3Int.forward);
    }

    // Get the position of the chunk containing this block.
    private static Vector3Int GetChunkPos(int x, int y, int z) => new()
    {
        x = x / Chunk.Size,
        y = y / Chunk.Size,
        z = z / Chunk.Size
    };
    
    // Get the location of this block inside the chunk containing it.
    private static Vector3Int GetLocalPos(int x, int y, int z) => new()
    {
        x = x % Chunk.Size,
        y = y % Chunk.Size,
        z = z % Chunk.Size
    };

    private void MarkDirty(Vector3Int chunkPos)
    {
        if (chunks.ContainsKey(chunkPos)) chunks[chunkPos].data.MarkDirty();
    }
    
    private void InstantiateChunk(Vector3Int position)
    {
        Vector3 worldPos = position * Chunk.Size;

        GameObject chunkObj = Instantiate(chunkPrefab, worldPos, Quaternion.identity);
        ChunkRenderer chunkRenderer = chunkObj.GetComponent<ChunkRenderer>();
        chunkRenderer.material = textureArray.Material;
        ChunkData chunkData = new(this, position);
        
        chunks.Add(position, new Chunk(chunkData, chunkRenderer));
    }
}
