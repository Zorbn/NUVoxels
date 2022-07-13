public static class Block
{
    public const int BlockCount = 3;
    public const int SideCount = 6;

    public enum Id
    {
        Air,
        Grass,
        WeirdGrass
    }

    public static int GetTextureId(Id blockId, Direction.Axis sideId)
    {
        return (int)sideId + (int)blockId * SideCount;
    }
}
