using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
    private HexCell[] _cells;
    private HexMesh _hexMesh;
    private Canvas _gridCanvas;
    
    private void Awake()
    {
        _gridCanvas = GetComponentInChildren<Canvas>();
        _hexMesh = GetComponentInChildren<HexMesh>();
        
        _cells = new HexCell[HexMetrics.CHUNK_SIZE_X * HexMetrics.CHUNK_SIZE_Z];
        ShowUI(false);
    }
    
    public void AddCell(int index, HexCell cell)
    {
        _cells[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(_gridCanvas.transform, false);
    }

    public void Refresh()
    {
        enabled = true;
    }

    private void LateUpdate()
    {
        _hexMesh.Triangulate(_cells);
        enabled = false;
    }

    public void ShowUI(bool visible)
    {
        _gridCanvas.gameObject.SetActive(visible);
    }
}
