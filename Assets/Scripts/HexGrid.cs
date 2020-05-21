using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    #region SIZE

    public int chunkCountX = 4, chunkCountZ = 3;
    private int _cellCountX, _cellCountZ;

    #endregion

    #region PREFAB

    public HexGridChunk chunkPrefab;
    public HexCell cellPrefab;
    public TextMeshProUGUI cellLabelPrefab;

    #endregion

    #region COLORS

    public Color defaultColor;

    #endregion

    #region NOISE

    public Texture2D noiseSource;

    #endregion
    
    #region REFERENCES

    private HexGridChunk[] _chunks;
    private HexCell[] _cells;

    #endregion

    void Awake()
    {
        HexMetrics.noiseSource = noiseSource;
        
         _cellCountX = chunkCountX * HexMetrics.CHUNK_SIZE_X;
        _cellCountZ = chunkCountZ * HexMetrics.CHUNK_SIZE_Z;

        CreateChunks();
        CreateCells();
    }

    private void CreateChunks()
    {
        _chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++) 
        {
            for (int x = 0; x < chunkCountX; x++) 
            {
                HexGridChunk chunk = _chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }

    private void CreateCells()
    {
        _cells = new HexCell[_cellCountX * _cellCountZ];
        
        for (int z = 0, i = 0; z < _cellCountZ; z++) 
        {
            for (int x = 0; x < _cellCountX; x++) 
            {
                CreateCell(x, z, i++);
            }
        }
    }
    
    void CreateCell (int x, int z, int i) 
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.INNER_RADIUS * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.OUTER_RADIUS * 1.5f);

        HexCell cell = _cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.Color = defaultColor;
        cell.name = "Hex Cell: " + cell.coordinates.ToString();

        if (x > 0) 
        {
            cell.SetNeighbor(HexDirection.W, _cells[i - 1]);
        }

        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, _cells[i - _cellCountX]);

                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[i - _cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, _cells[i - _cellCountX]);
                if (x < _cellCountX - 1) 
                {
                    cell.SetNeighbor(HexDirection.SE, _cells[i - _cellCountX + 1]);
                }
            }
        }
        
        TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab/*, _gridCanvas.transform, false*/);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform;

        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }

    private void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.CHUNK_SIZE_X;
        int chunkZ = z / HexMetrics.CHUNK_SIZE_Z;
        HexGridChunk chunk = _chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.CHUNK_SIZE_X;
        int localZ = z - chunkZ * HexMetrics.CHUNK_SIZE_Z;
        chunk.AddCell(localX + localZ * HexMetrics.CHUNK_SIZE_X, cell);
    }


    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * _cellCountX + coordinates.Z / 2;
        return _cells[index];
    }
    
    void OnEnable () 
    {
        HexMetrics.noiseSource = noiseSource;
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;

        if (z < 0 || z >= _cellCountZ)
        {
            return null;
        }
        
        int x = coordinates.X + z / 2;
        
        if (x < 0 || x >= _cellCountX)
        {
            return null;
        }
        
        return _cells[x + z * _cellCountX];
    }

    public void ShowUi(bool visible)
    {
        foreach (var t in _chunks)
        {
            t.ShowUI(visible);
        }
    }
}
