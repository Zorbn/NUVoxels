using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class World : MonoBehaviour
{
    public const int MapWidthInChunks = 12;
    public const int MapHeightInChunks = 8;
    public const float GroundLevel = ChunkRenderer.Size * MapHeightInChunks * 0.5f;
    
    // TODO List:
    // Consider splitting chunk renderer into chunk renderer and chunk data classes
    // Consider how to path-find on this type of terrain, maybe:
    // Greedy Best First Search: https://www.redblobgames.com/pathfinding/a-star/introduction.html?
    // Actually make a game with this! (Spelunky like levels in 3d with buildings and gameplay more like streets of rogue)
    
    [SerializeField] private GameObject chunkPrefab;
    
    private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private Vector2[] textureOffsets;
    private Material atlasMaterial;
    private Texture2DArray texArray;

    private Dictionary<Vector3Int, ChunkRenderer> ChunkRenderers { get; } = new();

    private void Start()
    {
        texArray = new Texture2DArray(CubeMesh.CubeTexWidth, CubeMesh.CubeTexHeight, 
            Block.BlockCount * Block.SideCount, TextureFormat.ARGB32, 3, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

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
        
        atlasMaterial = new Material(Shader.Find("Custom/StandardTextureArray"));
        atlasMaterial.SetTexture(MainTex, texArray);
        atlasMaterial.SetFloat(Glossiness, 0);

        for (int x = 0; x < MapWidthInChunks; x++)
        {
            for (int z = 0; z < MapWidthInChunks; z++)
            {
                for (int y = 0; y < MapHeightInChunks; y++)
                {
                    InstantiateChunk(new Vector3Int(x, y, z));
                }
            }
        }

        Stopwatch watch = new();
        watch.Start();
        
        ParallelGenerateChunks();
        
        watch.Stop();
        Debug.Log($"Generation time: {watch.ElapsedMilliseconds}");
    }

    private void ParallelGenerateChunks()
    {
        Parallel.ForEach(ChunkRenderers, pair =>
        {
            pair.Value.GenerateData();
        });
        
        Parallel.ForEach(ChunkRenderers, pair =>
        {
            pair.Value.GenerateMeshData();
        });
        
        foreach (KeyValuePair<Vector3Int,ChunkRenderer> chunkRenderer in ChunkRenderers)
        {
            chunkRenderer.Value.UpdateMesh();
        }
    }

    public Block.Id GetBlock(int x, int y, int z)
    {
        int chunkX = x / ChunkRenderer.Size,
            chunkY = y / ChunkRenderer.Size,
            chunkZ = z / ChunkRenderer.Size;

        Vector3Int chunkPos = new(chunkX, chunkY, chunkZ);

        if (!ChunkRenderers.ContainsKey(chunkPos)) return Block.Id.Air;

        int localX = x % ChunkRenderer.Size,
            localY = y % ChunkRenderer.Size,
            localZ = z % ChunkRenderer.Size;
        
        return ChunkRenderers[chunkPos].GetBlock(localX, localY, localZ);
    }
    
    // TODO: Consolidate duplicate code between GetBlock and SetBlock.
    // TODO: Consider adding Vector3Int versions of functions like these.
    public void SetBlock(Block.Id blockId, int x, int y, int z)
    {
        int chunkX = x / ChunkRenderer.Size,
            chunkY = y / ChunkRenderer.Size,
            chunkZ = z / ChunkRenderer.Size;

        Vector3Int chunkPos = new(chunkX, chunkY, chunkZ);

        if (!ChunkRenderers.ContainsKey(chunkPos)) return;

        int localX = x % ChunkRenderer.Size,
            localY = y % ChunkRenderer.Size,
            localZ = z % ChunkRenderer.Size;

        ChunkRenderer chunkRenderer = ChunkRenderers[chunkPos];
        chunkRenderer.SetBlock(blockId, localX, localY, localZ);
        
        // TODO: Add this to a queue and do it off-thread.
        chunkRenderer.GenerateMeshData();
        chunkRenderer.UpdateMesh();
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
        
        ChunkRenderers.Add(position, chunkRenderer);
    }
}
