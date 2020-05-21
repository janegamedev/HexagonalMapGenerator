using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;
    public HexGrid hexGrid;
    
    private Color _activeColor;
    private int _activeElevation;
    private int _brushSize;

    private bool _applyColor, _applyElevation;

    private void Awake()
    {
        SelectColor(0);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }

   private void HandleInput()
   {
       Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
       if (Physics.Raycast(ray, out RaycastHit hit))
       {
           EditCells(hexGrid.GetCell(hit.point));
       }
   }

   private void EditCells(HexCell center)
   {
       int centerX = center.coordinates.X;
       int centerZ = center.coordinates.Z;

       for (int r = 0, z = centerZ - _brushSize; z <= centerZ ; z++, r++)
       {
           for (int x = centerX - r; x <= centerX + _brushSize; x++)
           {
               EditCell(hexGrid.GetCell(new HexCoordinates(x,z)));
           }
       }
       
       for (int r = 0, z = centerZ + _brushSize; z > centerZ ; z--, r++)
       {
           for (int x = centerX - _brushSize; x <= centerX + r; x++)
           {
               EditCell(hexGrid.GetCell(new HexCoordinates(x,z)));
           }
       }
   }
   
   private void EditCell(HexCell cell)
   {
       if (cell)
       {
           if (_applyColor)
           {
               cell.color = _activeColor;
           }

           if (_applyElevation)
           {
               cell.Elevation = _activeElevation;
           }
       }
   }

    public void SelectColor(int index)
    {
        _applyColor = index >= 0;

        if (_applyColor)
        {
            _activeColor = colors[index];
        }
    }

    public void SetApplyElevation(bool toggle)
    {
        _applyElevation = toggle;
    }

    public void SetElevation(float elevation)
    {
        _activeElevation = (int) elevation;
    }

    public void SetBrushSize(float size)
    {
        _brushSize = (int) size;
    }

    public void ShowUi(bool visible)
    {
        hexGrid.ShowUi(visible);
    }
}
