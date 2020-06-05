﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private static List<Vector3> _verts = new List<Vector3>();
    private static List<int> _tris = new List<int>();
    private static List<Color> _colors = new List<Color>();
    
    private Mesh _hexMesh;
    private MeshCollider _meshCollider;
    
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
                Triangulate(d, cell);
            }
        }

        _hexMesh.vertices = _verts.ToArray();
        _hexMesh.triangles = _tris.ToArray();
        _hexMesh.colors = _colors.ToArray();
        _hexMesh.RecalculateNormals();
        _meshCollider.sharedMesh = _hexMesh;
    }

    private void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.Position;
        EdgeVertices e = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));

        if (cell.HasRiverThroughEdge(direction))
        {
            e.v3.y = cell.StreamBedY;
        }
        
        TriangulateEdgeFan(center, e, cell.color);

        if (direction <= HexDirection.SE) 
        {
            TriangulateConnection(direction, cell, e);
        }
    }
    
    private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
        {
            return;
        }
        
        Vector3 bridge = HexMetrics.GetBridge(direction);
        bridge.y = neighbor.Position.y - cell.Position.y;
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

        if (cell.HasRiverThroughEdge(direction))
        {
            e2.v3.y = neighbor.StreamBedY;
        }
        
        if (cell.GetEdgeType(direction) == HexEdgeType.SLOPE) 
        {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
        }
        else 
        {
            TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);
        }

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
       
        if (direction <= HexDirection.E && nextNeighbor != null) 
        {
            Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation) 
            {
                if (cell.Elevation <= nextNeighbor.Elevation) 
                {
                    TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                }
                else 
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation) 
            {
                TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            }
            else 
            {
                TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }
        }
    }
    
    private void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right,
        HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.SLOPE) 
        {
            if (rightEdgeType == HexEdgeType.SLOPE) 
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.FLAT) 
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else 
            {
                TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (rightEdgeType == HexEdgeType.SLOPE) 
        {
            if (leftEdgeType == HexEdgeType.FLAT) 
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else 
            {
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.SLOPE) 
        {
            if (leftCell.Elevation < rightCell.Elevation) 
            {
                TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else 
            {
                TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }
        else {
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
        }
    }
    
    private void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerpColor(beginCell.Color, endCell.Color, 1);

        TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

        for (int i = 2; i < HexMetrics.TERRACE_STEPS; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerpColor(beginCell.Color, endCell.Color, i);
            
            TriangulateEdgeStrip(e1, c1, e2, c2);
        }
        
        TriangulateEdgeStrip(e2, c2, end, endCell.Color);
    }
    
    private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        
        Color c3 = HexMetrics.TerraceLerpColor(beginCell.Color, leftCell.Color, 1);
        Color c4 = HexMetrics.TerraceLerpColor(beginCell.Color, rightCell.Color, 1);
        
        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.Color, c3, c4);

        for (int i = 0; i < HexMetrics.TERRACE_STEPS; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;

            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerpColor(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerpColor(beginCell.color, rightCell.color, i);
            
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }
        
        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
    }
    
    private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0) 
        {
            b = -b;
        }
        
        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.SLOPE)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }
    
    private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0) 
        {
            b = -b;
        }
        
        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);
        
        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);
        
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.SLOPE)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }
    
    private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColor)
    {
        Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerpColor(beginCell.Color, leftCell.Color, 1);

        AddTriangleUnperturbed(Perturb(begin), v2, boundary);
        AddTriangleColor(beginCell.Color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.TERRACE_STEPS; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            
            v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerpColor(beginCell.Color, leftCell.Color, i);
            
            AddTriangleUnperturbed(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }
        
        AddTriangleUnperturbed(v2, Perturb(left), boundary);
        AddTriangleColor(c2, leftCell.Color, boundaryColor);
    }


    private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        AddTriangle(center, edge.v1, edge.v2);
        AddTriangleColor(color);
        AddTriangle(center, edge.v2, edge.v3);
        AddTriangleColor(color);
        AddTriangle(center, edge.v3, edge.v4);
        AddTriangleColor(color);
        AddTriangle(center, edge.v4, edge.v5);
        AddTriangleColor(color);
    }

    private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
    {
        AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        AddQuadColor(c1, c2);
        AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        AddQuadColor(c1, c2);
        AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        AddQuadColor(c1, c2);
        AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
        AddQuadColor(c1, c2);
    }
    
    private void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) 
    {
        int vertexIndex = _verts.Count;
        _verts.Add(Perturb(v1));
        _verts.Add(Perturb(v2));
        _verts.Add(Perturb(v3));
        _tris.Add(vertexIndex);
        _tris.Add(vertexIndex + 1);
        _tris.Add(vertexIndex + 2);
    }
    
    private void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = _verts.Count;
        _verts.Add(v1);
        _verts.Add(v2);
        _verts.Add(v3);
        _tris.Add(vertexIndex);
        _tris.Add(vertexIndex + 1);
        _tris.Add(vertexIndex + 2);
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
        int vertexIndex = _verts.Count;
        _verts.Add(Perturb(v1));
        _verts.Add(Perturb(v2));
        _verts.Add(Perturb(v3));
        _verts.Add(Perturb(v4));
        
        _tris.Add(vertexIndex);
        _tris.Add(vertexIndex + 2);
        _tris.Add(vertexIndex + 1);
        _tris.Add(vertexIndex + 1);
        _tris.Add(vertexIndex + 2);
        _tris.Add(vertexIndex + 3);
    }
    
    private void AddQuadColor(Color c1, Color c2)
    {
        _colors.Add(c1);
        _colors.Add(c1);
        _colors.Add(c2);
        _colors.Add(c2);
    }

    private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        _colors.Add(c1);
        _colors.Add(c2);
        _colors.Add(c3);
        _colors.Add(c4);
    }
    
    private Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = HexMetrics.SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * HexMetrics.CELL_PERTURB_STRENGTH;
        position.z += (sample.z * 2f - 1f) * HexMetrics.CELL_PERTURB_STRENGTH;
        return position;
    }
}
