using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public Color color;
    [SerializeField] private HexCell[] neighbors;

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int) direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }
}

public static class HexMetrics
{
    public const float SOLID_FACTOR = 0.75f;
    public const float BLEND_FACTOR = 1f - SOLID_FACTOR;
    
    public const float OUTER_RADIUS = 10f;
    public const float INNER_RADIUS = OUTER_RADIUS * 0.866025404f;
    
    public static readonly Vector3[] Corners = {
        new Vector3(0f, 0f, OUTER_RADIUS),
        new Vector3(INNER_RADIUS, 0f, 0.5f * OUTER_RADIUS),
        new Vector3(INNER_RADIUS, 0f, -0.5f * OUTER_RADIUS),
        new Vector3(0f, 0f, -OUTER_RADIUS),
        new Vector3(-INNER_RADIUS, 0f, -0.5f * OUTER_RADIUS),
        new Vector3(-INNER_RADIUS, 0f, 0.5f * OUTER_RADIUS),
        new Vector3(0f, 0f, OUTER_RADIUS) // last vertex from first angle
    };

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return Corners[(int) direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return Corners[(int) direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return Corners[(int) direction] * SOLID_FACTOR;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return Corners[(int) direction + 1] * SOLID_FACTOR;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (Corners[(int) direction] + Corners[(int) direction + 1]) * BLEND_FACTOR;
    }
}

