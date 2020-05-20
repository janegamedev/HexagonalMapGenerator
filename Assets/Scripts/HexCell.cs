using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public Color color;
    [SerializeField] private HexCell[] neighbors;

    public RectTransform uiRect;
    public int Elevation
    {
        get => elevation;
        set
        {
            elevation = value;
            Vector3 positon = transform.localPosition;
            positon.y = value * HexMetrics.ELEVATION_STEP;
            transform.localPosition = positon;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = elevation * -HexMetrics.ELEVATION_STEP;
            uiRect.localPosition = uiPosition;
        }
    }

    private int elevation;

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int) direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int) direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.Elevation);
    }
}



