using System;

public class SharedRandom
{
    [ThreadStatic] private static Random DefaultRand;
    public static Random Default => DefaultRand ??= new Random();
}