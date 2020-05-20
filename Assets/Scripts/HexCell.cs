using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public Color color;
}

public static class HexMetrics
{
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
}