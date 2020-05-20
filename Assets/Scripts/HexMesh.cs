using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{

    private Mesh _hexMesh;
    private MeshCollider _meshCollider;

    private List<Vector3> _verts = new List<Vector3>();
    private List<int> _tris = new List<int>();
    private List<Color> _colors = new List<Color>();

    
    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = _hexMesh = new Mesh();
        _meshCollider = gameObject.AddComponent<MeshCollider>();
        _hexMesh.name = "Hex Mesh";
    }

    public void Triangulate(HexCell[] cells)
    {
        _hexMesh.Clear();
        _verts.Clear();
        _tris.Clear();
        _colors.Clear();

        foreach (var cell in cells)
        {
             for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
             {
                Triangulate(cell, d);
             }
        }

        _hexMesh.vertices = _verts.ToArray();
        _hexMesh.triangles = _tris.ToArray();
        _hexMesh.colors = _colors.ToArray();
        _hexMesh.RecalculateNormals();
        _meshCollider.sharedMesh = _hexMesh;
    }

    private void Triangulate(HexCell cell, HexDirection direction)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);
        
        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.color);

        if (direction <= HexDirection.SE) 
        {
            TriangulateConnection(direction, cell, v1, v2);
        }
    }

    private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        HexCell neighbor = cell.GetNeighbor(direction); 

        if (neighbor == null)
        {
            return;
        }
        
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        
        AddQuad(v1, v2, v3, v4);
        AddQuadColor(cell.color, neighbor.color);
        
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
    }

    private void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) 
    {
        int vertexIndex = _verts.Count;
        _verts.Add(v1);
        _verts.Add(v2);
        _verts.Add(v3);

        for (int i = 0; i < 3; i++)
        {
            _tris.Add(vertexIndex + i);
        }
    }

    private void AddTriangleColor(Color c1)
    {
        for (int i = 0; i < 3; i++)
        {
            _colors.Add(c1);
        }
    }

    private void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        _colors.Add(c1); 
        _colors.Add(c2);
        _colors.Add(c3);
    }

    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int index = _verts.Count;
        
        _verts.Add(v1);
        _verts.Add(v2);
        _verts.Add(v3);
        _verts.Add(v4);
        
        _tris.Add(index);
        _tris.Add(index + 2);
        _tris.Add(index + 1);
        _tris.Add(index + 1);
        _tris.Add(index + 2);
        _tris.Add(index + 3);
    }

    private void AddQuadColor(Color c1, Color c2)
    {
        _colors.Add(c1);
        _colors.Add(c1);
        _colors.Add(c2);
        _colors.Add(c2);
    }
}
