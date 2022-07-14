using System.IO;
using UnityEngine;

public class VoxelTextureArray
{
    private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    public Material Material { get; private set; }
    private Texture2DArray array;

    public void Initialize()
    {
        array = new Texture2DArray(CubeMesh.CubeTexWidth, CubeMesh.CubeTexHeight, 
            Block.BlockCount * Block.SideCount, TextureFormat.ARGB32, 3, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        FillTextureArray();

        Material = new Material(Shader.Find("Custom/StandardTextureArray"));
        Material.SetTexture(MainTex, array);
        Material.SetFloat(Glossiness, 0);
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
                string texPath = $"Blocks/{blockName}/{sideName}";
                Texture2D texture2D = Resources.Load<Texture2D>(texPath);

                if (texture2D == null) throw new FileLoadException($"Failed to load texture from '{texPath}'");

                array.SetPixels(texture2D.GetPixels(), Block.GetTextureId(blockId, sideId));
            }
        }

        array.Apply();
    }
}
