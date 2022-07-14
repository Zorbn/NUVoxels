public class Chunk
{
    public const int Size = 32;
    public const int Size1D = Size * Size * Size;
    
    public readonly ChunkData data;
    public readonly ChunkRenderer renderer;

    public Chunk(ChunkData data, ChunkRenderer renderer)
    {
        this.data = data;
        this.renderer = renderer;
    }
}
