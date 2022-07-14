using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
    public bool IsDirty { get; private set; }

    public readonly List<Vector3> vertices = new();
    public readonly List<int> indices = new();
    public readonly List<Vector3> uv = new();
    
    private int indexOffset;
    
    private readonly World world;
    private readonly Vector3Int position;

    private readonly Block.Id[] blocks = new Block.Id[Chunk.Size * Chunk.Size * Chunk.Size];
    
    public ChunkData(World world, Vector3Int position)
    {
        this.world = world;
        this.position = position;
    }
    
    public void Generate(Action<Vector3Int, System.Random> generator)
    {
        Vector3Int chunkWorldPos = position * Chunk.Size;
        
        for (int i = 0; i < Chunk.Size1D; i++)
        {
            Vector3Int localPos = i.To3D(Chunk.Size).ToIVec();
            generator.Invoke(chunkWorldPos + localPos, SharedRandom.Default);
        }
    }

    public void GenerateMesh()
    {
        vertices.Clear();
        indices.Clear();
        uv.Clear();
        indexOffset = 0;

        for (int i = 0; i < Chunk.Size1D; i++)
        {
            (int x, int y, int z) = i.To3D(Chunk.Size);

            Block.Id id = GetBlock(x, y, z);
            if (id != Block.Id.Air) GenCube(id, x, y, z);
        }

        IsDirty = false;
    }

    private void GenCube(Block.Id blockId, int x, int y, int z)
    {
        for (int i = 0; i < 6; i++) GenFaceIfNecessary(blockId, x, y, z, (Direction.Axis)i);
    }

    public Block.Id GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);
    
    public Block.Id GetBlock(int x, int y, int z) =>
        IsBlockInBounds(x, y, z) ? blocks[(x, y, z).To1D(Chunk.Size)] : Block.Id.Air;
    
    public void SetBlock(Block.Id blockId, Vector3Int pos) => SetBlock(blockId, pos.x, pos.y, pos.z);
    
    public void SetBlock(Block.Id blockId, int x, int y, int z)
    {
        if (!IsBlockInBounds(x, y, z)) return;

        blocks[(x, y, z).To1D(Chunk.Size)] = blockId;
        IsDirty = true;
    }

    public void MarkDirty() => IsDirty = true;

    private static bool IsBlockInBounds(int x, int y, int z) => x is >= 0 and < Chunk.Size &&
                                                                y is >= 0 and < Chunk.Size &&
                                                                z is >= 0 and < Chunk.Size;

    private void GenFaceIfNecessary(Block.Id blockId, int x, int y, int z, Direction.Axis direction)
    {
        Vector3 offset = Direction.AxisToVec(direction);
        Vector3Int pos = new(x, y, z);
        
        if (world.GetBlock(pos + Vector3Int.RoundToInt(offset) + position * Chunk.Size) == Block.Id.Air)
            GenFace(blockId, x, y, z, direction);
    }

    private void GenFace(Block.Id blockId, int x, int y, int z, Direction.Axis direction)
    {
        Vector3 facePos = new(x, y, z);
        int faceVerticesLength = CubeMesh.FaceVertices[direction].Length;
        
        for (int i = 0; i < faceVerticesLength; i++)
        {
            vertices.Add(facePos + CubeMesh.FaceVertices[direction][i]);
            
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
