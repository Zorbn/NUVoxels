using System;
using UnityEngine;

public static class VoxelRay
{
    // Ray-casting through voxels, based on a method used by jdh.
    public static (Block.Id blockId, Vector3Int blockPos, Vector3Int hitNormal)
        Cast(World world, Vector3 origin, Vector3 direction, float distance)
    {
        // Track the block position of the ray.
        Vector3Int blockPos = Vector3Int.FloorToInt(origin);
        // Track the amount the ray can travel in each direction per step.
        Vector3Int step = new()
        {
            x = Math.Sign(direction.x),
            y = Math.Sign(direction.y),
            z = Math.Sign(direction.z)
        };
        
        // The ray should always move towards the closest full block in the direction its headed.
        // First the ray should travel to the nearest full block position from its origin, then the
        // ray should continue traveling in its direction by increments of 1 block on each axis.
        
        // Find the smallest distance the ray needs to travel in each component of its direction
        // to reach its first exact (integer) block position.
        Vector3 tMax = IntBound(origin, direction);
        // Find the distance the ray needs to travel to move by one full block on each component (x, y, z).
        Vector3 tDelta = VecDiv(step, direction);
        
        // Store the reverse of the most recent step, once a block has been hit, this will be the hit normal.
        Vector3Int hitNormal = Vector3Int.zero;
        
        while (true)
        {
            Block.Id blockId = world.GetBlock(blockPos);
            if (blockId != Block.Id.Air) return (blockId, blockPos, hitNormal);

            int i = GetSmallestComponent(tMax); // Move towards the next closest block.

            if (tMax[i] > distance) break;

            blockPos[i] += step[i];
            tMax[i] += tDelta[i];
            hitNormal = Vector3Int.zero;
            hitNormal[i] = -step[i];
        }

        return (Block.Id.Air, blockPos, Vector3Int.zero);
    }

    private static int GetSmallestComponent(Vector3 vec)
    {
        if (vec.x < vec.y && vec.x < vec.z) return 0;
        return vec.y < vec.z ? 1 : 2;
    }
    
    // Find smallest multiple of direction that will make start reach the next block location.
    private static Vector3 IntBound(Vector3 start, Vector3 direction)
    {
        Vector3 t = Vector3.zero;

        for (int i = 0; i < 3; i++)
        {
            t[i] = (direction[i] > 0 ? Mathf.Ceil(start[i]) - start[i] : start[i] - Mathf.Floor(start[i])) / Mathf.Abs(direction[i]);
        }

        return t;
    }

    private static Vector3 VecDiv(Vector3 a, Vector3 b)
    {
        a.x /= b.x;
        a.y /= b.y;
        a.z /= b.z;

        return a;
    }
}
