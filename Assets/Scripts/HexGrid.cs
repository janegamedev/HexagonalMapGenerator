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
    public Color touchedColor;

    #endregion
    
    #region REFERENCES

    private HexCell[] _cells;
    private Canvas _gridCanvas;
    private HexMesh _hexMesh;

    #endregion
    
    
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
        
        TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab, _gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
    }
    

    public void ColorCell(Vector3 position, Color color)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        HexCell cell = _cells[index];
        cell.color = color;
        _hexMesh.Triangulate(_cells);
    }
}
