using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public Color color;
    public HexGridChunk chunk;
    public RectTransform uiRect;
    private bool _hasIncomingRiver, _hasOutgoingRiver;
    private HexDirection _incomingRiver, _outgoingRiver;
    [SerializeField] private HexCell[] neighbors;

    public bool HasRiver => _hasIncomingRiver || _hasOutgoingRiver;
    public bool HasRiverBeginOrEnd => _hasIncomingRiver != _hasOutgoingRiver;
    public bool HasIncomingRiver => _hasIncomingRiver;
    public bool HasOutgoingRiver => _hasOutgoingRiver;
    public HexDirection IncomingRiver => _incomingRiver;
    public HexDirection OutgoingRiver => _outgoingRiver;

    public float StreamBedY => (_elevation + HexMetrics.STREAM_BED_ELEVATION_OFFSET) * HexMetrics.ELEVATION_STEP;

    public int Elevation
    {
        get => _elevation;
        set
        {
            if (_elevation == value) {
                return;
            }
            _elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.ELEVATION_STEP;
            position.y +=
                (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.ELEVATION_PERTURB_STRENGTH;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            if (_hasOutgoingRiver && _elevation < GetNeighbor(_outgoingRiver).Elevation)
            {
                RemoveOutgoingRiver();
            }

            if (_hasIncomingRiver && _elevation > GetNeighbor(_incomingRiver).Elevation)
            {
                RemoveIncomingRiver();
            }
            
            Refresh();
        }
    }

    public Color Color
    {
        get => color;
        set
        {
            if (color == value)
            {
                return;
            }

            color = value;
            Refresh();
        }
    }
    private int _elevation = int.MinValue;

    public Vector3 Position => transform.localPosition;

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return _hasIncomingRiver && _incomingRiver == direction || _hasOutgoingRiver && _outgoingRiver == direction;
    }

    private void RemoveOutgoingRiver()
    {
        if (!_hasOutgoingRiver)
        {
            return;
        }

        _hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(_outgoingRiver);
        neighbor._hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    private void RemoveIncomingRiver()
    {
        if (!_hasIncomingRiver)
        {
            return;
        }

        _hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(_incomingRiver);
        neighbor._hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (_hasOutgoingRiver && _outgoingRiver == direction)
        {
            return;
        }

        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || _elevation < neighbor.Elevation)
        {
            return;
        }
        
        RemoveOutgoingRiver();
        if (_hasIncomingRiver && _incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }

        _hasOutgoingRiver = true;
        _outgoingRiver = direction;
        RefreshSelfOnly();
        
        neighbor.RemoveIncomingRiver();
        neighbor._hasIncomingRiver = true;
        neighbor._incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }

    private void RefreshSelfOnly()
    {
        chunk.Refresh();
    }

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

    private void Refresh()
    {
        if (!chunk) return;

        chunk.Refresh();
        
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null && neighbor.chunk != null)
            {
                neighbor.chunk.Refresh();
            }
        }
    }
    
    
}



