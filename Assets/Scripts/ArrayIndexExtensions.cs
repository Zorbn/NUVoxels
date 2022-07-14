public static class ArrayIndexExtensions
{
    public static int To1D(this (int x, int y, int z) i, int Size) => i.x + i.y * Size + i.z * Size * Size;

    public static (int x, int y, int z) To3D(this int i, int Size) =>
        (i % Size, i / Size % Size, i / (Size * Size));
    
    public static (int x, int y, int z) To3D(this int i, int xSize, int ySize) =>
        (i % xSize, i / xSize % ySize, i / (xSize * ySize));
}