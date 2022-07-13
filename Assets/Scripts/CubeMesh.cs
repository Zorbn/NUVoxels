using System.Collections.Generic;
using UnityEngine;

public static class CubeMesh
{
    public const int CubeTexWidth = 16;
    public const int CubeTexHeight = 16;

    public static readonly Dictionary<Direction.Axis, Vector3[]> FaceVertices = new()
    {
        {
            Direction.Axis.XPos,
            new[] {
                new Vector3(1f, 0f,  0f),
                new Vector3(1f, 0f,   1f),
                new Vector3(1f,  1f,   1f),
                new Vector3(1f,  1f,  0f)
            }
        },
        {
            Direction.Axis.XNeg,
            new[] {
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f,  1f),
                new Vector3(0f,  1f,  1f),
                new Vector3(0f,  1f, 0f)
            }
        },
        {
            Direction.Axis.YPos,
            new[] {
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f,  1f),
                new Vector3( 1f, 1f,  1f),
                new Vector3( 1f, 1f, 0f)
            }
        },
        {
            Direction.Axis.YNeg,
            new[] {
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f,  1f),
                new Vector3( 1f, 0f,  1f),
                new Vector3( 1f, 0f, 0f)
            }
        },
        {
            Direction.Axis.ZPos,
            new[] {
                new Vector3(0f, 0f, 1f),
                new Vector3(0f,  1f, 1f),
                new Vector3( 1f,  1f, 1f),
                new Vector3( 1f, 0f, 1f)
            }
        },
        {
            Direction.Axis.ZNeg,
            new[] {
                new Vector3(0f, 0f, 0f),
                new Vector3(0f,  1f, 0f),
                new Vector3( 1f,  1f, 0f),
                new Vector3( 1f, 0f, 0f)
            }
        }
    };
    
    /*
    public static readonly Dictionary<Direction.Axis, Vector3[]> FaceVertices = new()
    {
        {
            Direction.Axis.XPos,
            new[] {
                new Vector3(0.5f, -0.5f,  -0.5f),
                new Vector3(0.5f, -0.5f,   0.5f),
                new Vector3(0.5f,  0.5f,   0.5f),
                new Vector3(0.5f,  0.5f,  -0.5f)
            }
        },
        {
            Direction.Axis.XNeg,
            new[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f)
            }
        },
        {
            Direction.Axis.YPos,
            new[] {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f,  0.5f),
                new Vector3( 0.5f, 0.5f,  0.5f),
                new Vector3( 0.5f, 0.5f, -0.5f)
            }
        },
        {
            Direction.Axis.YNeg,
            new[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f)
            }
        },
        {
            Direction.Axis.ZPos,
            new[] {
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f,  0.5f, 0.5f),
                new Vector3( 0.5f,  0.5f, 0.5f),
                new Vector3( 0.5f, -0.5f, 0.5f)
            }
        },
        {
            Direction.Axis.ZNeg,
            new[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f)
            }
        }
    };
     */

    public static readonly Dictionary<Direction.Axis, int[]> FaceIndices = new()
    {
        {
            Direction.Axis.XPos,
            new[]
            {
                1, 3, 2,
                1, 0, 3
            }
        },
        {
            Direction.Axis.XNeg,
            new[]
            {
                0, 2, 3,
                0, 1, 2
            }
        },
        {
            Direction.Axis.YPos,
            new[]
            {
                0, 2, 3,
                0, 1, 2
            }
        },
        {
            Direction.Axis.YNeg,
            new[]
            {
                1, 3, 2,
                1, 0, 3
            }
        },
        {
            Direction.Axis.ZPos,
            new[]
            {
                1, 3, 2,
                1, 0, 3
            }
        },
        {
            Direction.Axis.ZNeg,
            new[]
            {
                0, 2, 3,
                0, 1, 2
            }
        }
    };

    public static readonly Dictionary<Direction.Axis, Vector2[]> FaceUV = new()
    {
        {
            Direction.Axis.XPos,
            new[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            }
        },
        {
            Direction.Axis.XNeg,
            new[] {
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            }
        },
        {
            Direction.Axis.YPos,
            new[] {
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0)
            }
        },
        {
            Direction.Axis.YNeg,
            new[] {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            }
        },
        {
            Direction.Axis.ZPos,
            new[] {
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0)
            }
        },
        {
            Direction.Axis.ZNeg,
            new[] {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0)
            }
        }
    };
}