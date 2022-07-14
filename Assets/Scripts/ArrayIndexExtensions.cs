using UnityEngine;

public static class ArrayIndexExtensions
{
    public static int To1D(this (int x, int y, int z) i, int size) => i.x + i.y * size + i.z * size * size;

    public static (int x, int y, int z) To3D(this int i, int size) =>
        (i % size, i / size % size, i / (size * size));
    
    public static (int x, int y, int z) To3D(this int i, int xSize, int ySize) =>
        (i % xSize, i / xSize % ySize, i / (xSize * ySize));

    public static Vector3Int ToIVec(this (int x, int y, int z) i) => new(i.x, i.y, i.z);
}