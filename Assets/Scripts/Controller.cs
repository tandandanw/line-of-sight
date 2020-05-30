#define DEBUG_DRAW 

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    public Vector2Int BoundsMin; // Left Bottom
    public Vector2Int BoundsMax; // Right Top
    public float BorderLength;
    public float LineShowingTime;

    public Transform viewerTransform;
    public GameObject TriangleFan;

    Tilemap tilemap;

    Vector3 viewerPosion;
    Vector2 viewerPosion2D;
    List<Edge> edges = new List<Edge>();
    List<Vector2> intersections = new List<Vector2>();

    Vector2Int[] directions = new Vector2Int[4]
    {
        new Vector2Int(1,0),
        new Vector2Int(0,-1),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
    };

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        viewerPosion = viewerTransform.position;
        viewerPosion2D = new Vector2(viewerPosion.x, viewerPosion.y);
    }

    void Update()
    {
        if (viewerTransform.position != viewerPosion)
        {
            viewerPosion = viewerTransform.position;
            viewerPosion2D = new Vector2(viewerPosion.x, viewerPosion.y);

            FindEdges();
            FindIntersection();
            ConstructTriangleFan();
        }
    }

    void FindEdges()
    {
        edges.Clear();

        for (int x = BoundsMin.x; x < BoundsMax.x; ++x)
        {
            for (int y = BoundsMin.y; y < BoundsMax.y; ++y)
            {
                var position = new Vector3Int(x, y, 0);
                var type = tilemap.GetTile(position).name;

                if (type == "GrassTile") continue;

                for (int i = 0; i < 4; ++i)
                {
                    var newPosition = new Vector3Int(x + directions[i].x, y + directions[i].y, 0);

                    if (IsOutOfBounds(newPosition)) continue;

                    var newType = tilemap.GetTile(newPosition).name;
                    if (newType != type)
                    {
                        // Find segments.
                        Vector2 start = Vector2.zero, end = Vector2.zero;

                        float posX = position.x;
                        float posY = position.y;
                        float dx = 0.5f;
                        float dy = 0.5f;

                        if (directions[i].x == 0)
                        {
                            if (directions[i].y < 0) dy = -dy;
                            start = new Vector2(posX - dx, posY + dy);
                            end = new Vector2(posX + dx, posY + dy);
                        }
                        else
                        {
                            if (directions[i].x < 0) dx = -dx;
                            start = new Vector2(posX + dx, posY - dy);
                            end = new Vector2(posX + dx, posY + dy);
                        }

                        // If it's a forward edge, draw it and its normal.
                        Vector3 view = (viewerPosion - position).normalized;
                        Vector3 normal = new Vector3(directions[i].x, directions[i].y, 0);
                        if (0 < Vector3.Dot(view, normal))
                        {
#if DEBUG_DRAW
                            // Draw normals.
                            Debug.DrawLine(start, end, Color.red, LineShowingTime);
                            // Draw normals.
                            Debug.DrawLine(position, newPosition, Color.blue, LineShowingTime);
#endif
                            var center = new Vector2((start.x + end.x) / 2, (start.y + end.y) / 2);
                            var edge = new Edge(start, end, center, directions[i]);
                            edges.Add(edge);
                        }

                    }
                }
            }
        }
    }

    void FindIntersection()
    {
        intersections.Clear();
        edges.Sort(CompareEdgeByDistance);

        for (double theta = 0; theta < 2 * Math.PI; theta += 0.05f)
        {
            Vector2 d = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
            Vector2 o = viewerPosion2D;
            float t1 = float.MaxValue;
            int i = 0;
            for (i = 0; i < edges.Count; ++i)
            {
                Vector2 s = edges[i].start;
                Vector2 e = (edges[i].end - edges[i].start).normalized;
                t1 = (e.y * s.x - e.y * o.x + e.x * o.y - e.x * s.y) / (e.y * d.x - e.x * d.y);
                float t2 = (d.y * o.x - d.y * s.x + d.x * s.y - d.x * o.y) / (d.y * e.x - d.x * e.y);
                if (0 <= t2 && t2 <= 1 && t1 > 0) break;
            }
            if (i >= edges.Count) t1 = BorderLength;
            var intersection = o + t1 * d;
            intersections.Add(intersection);

#if DEBUG_DRAW

            Debug.DrawLine(viewerPosion, intersection, Color.yellow, LineShowingTime);

#endif
        }
    }

    void ConstructTriangleFan()
    {
        TriangleFan.SetActive(false);
        TriangleFan.transform.position = Vector3.zero;

        Mesh mesh = TriangleFan.GetComponent<MeshFilter>().mesh;

        var vertices = new List<Vector3>();
        vertices.Add(new Vector3(viewerPosion.x, viewerPosion.y, -1));
        for (int i = 0; i < intersections.Count; ++i)
            vertices.Add(new Vector3(intersections[i].x, intersections[i].y, -1));

        mesh.vertices = vertices.ToArray();

        var indices = new List<int>();
        for (int i = 1; i < intersections.Count; ++i)
        {
            indices.Add(0);
            indices.Add(i + 1);
            indices.Add(i);
        }
        indices.Add(0);
        indices.Add(1);
        indices.Add(intersections.Count);

        mesh.triangles = indices.ToArray();

        TriangleFan.SetActive(true);
    }

    void Log(object o) => Debug.Log(o);

    bool IsOutOfBounds(Vector3 vector)
    {
        if ((vector.x >= BoundsMin.x && vector.x < BoundsMax.x) &&
                (vector.y >= BoundsMin.y && vector.y < BoundsMax.y))
            return false;
        return true;
    }

    private int CompareEdgeByDistance(Edge e1, Edge e2)
    {
        float distance1 = Vector2.Distance(e1.center, viewerPosion);
        float distance2 = Vector2.Distance(e2.center, viewerPosion);
        if (distance1 > distance2)
            return 1;
        else if (distance1 < distance2)
            return -1;
        else return 0;
    }
}
