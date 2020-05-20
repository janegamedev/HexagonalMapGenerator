using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    #region SIZE

    public int width = 6;
    public int height = 6;

    #endregion

    #region PREFAB

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

    private HexCell[] _cells;
    private Canvas _gridCanvas;
    private HexMesh _hexMesh;

    #endregion
    
    
    void OnEnable () 
    {
        HexMetrics.noiseSource = noiseSource;
    }
    
    void Awake()
    {
        _gridCanvas = GetComponentInChildren<Canvas>();
        _hexMesh = GetComponentInChildren<HexMesh>();
        
        
        _cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++) 
        {
            for (int x = 0; x < width; x++) 
            {
                CreateCell(x, z, i++);
            }
        }
    }
    
    void Start () 
    {
        _hexMesh.Triangulate(_cells);
    }
	
    void CreateCell (int x, int z, int index) 
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.INNER_RADIUS * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.OUTER_RADIUS * 1.5f);

        HexCell cell = _cells[index] = Instantiate<HexCell>(cellPrefab, transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;
        cell.name = "Hex Cell: " + cell.coordinates.ToString();

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, _cells[index- 1]);
        }

        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, _cells[index - width]);

                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[index - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, _cells[index - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, _cells[index - width + 1]);
                }
            }
        }
        
        TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab, _gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform;

        /*cell.Elevation = 0;*/
    }
    

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        return _cells[index];
    }

    public void Refresh()
    {
        _hexMesh.Triangulate(_cells);
    }
}
