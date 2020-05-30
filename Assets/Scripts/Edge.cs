using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public readonly Vector2 start;
    public readonly Vector2 end;
    public readonly Vector2 center;
    public readonly Vector2 normal;

    public Edge(Vector2 start, Vector2 end, Vector2 center, Vector2 normal)
    {
        this.start = start;
        this.end = end;
        this.center = center;
        this.normal = normal;
    }
}
