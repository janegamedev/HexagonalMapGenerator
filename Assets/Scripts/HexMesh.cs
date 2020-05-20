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
            Vector3 center = cell.transform.localPosition;

            for (int i = 0; i < 6; i++)
            {
                AddTriangle(
                    center,
                    center + HexMetrics.Corners[i],
                    center + HexMetrics.Corners[i + 1]
                );
                AddTriangleColor(cell.color);
            }
        }

        _hexMesh.vertices = _verts.ToArray();
        _hexMesh.triangles = _tris.ToArray();
        _hexMesh.colors = _colors.ToArray();
        _hexMesh.RecalculateNormals();
        _meshCollider.sharedMesh = _hexMesh;
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

    private void AddTriangleColor(Color color)
    {
        for (int i = 0; i < 3; i++)
        {
            _colors.Add(color);
        }
    }
}
