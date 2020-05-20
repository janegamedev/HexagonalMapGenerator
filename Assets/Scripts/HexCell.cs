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
        get => _elevation;
        set
        {
            _elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.ELEVATION_STEP;
            position.y +=
                (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.ELEVATION_PERTURB_STRENGTH;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = _elevation * -HexMetrics.ELEVATION_STEP;
            uiRect.localPosition = uiPosition;
        }
    }

    private int _elevation;

    public Vector3 Position => transform.localPosition;

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
        return HexMetrics.GetEdgeType(_elevation, neighbors[(int) direction]._elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(_elevation, otherCell.Elevation);
    }
}



