using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StitchingUILineRenderer : Graphic
{
    [SerializeField] private float thickness = 8f;
    [SerializeField] private float minPointDistance = 6f;

    private readonly List<Vector2> points = new List<Vector2>();

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }

    public void ClearLine()
    {
        points.Clear();
        SetVerticesDirty();
    }

    public void AddPoint(Vector2 point)
    {
        if (points.Count > 0 && Vector2.Distance(points[points.Count - 1], point) < minPointDistance)
            return;

        points.Add(point);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points.Count < 2)
            return;

        for (int i = 0; i < points.Count - 1; i++)
            AddLineSegment(vh, points[i], points[i + 1]);
    }

    private void AddLineSegment(VertexHelper vh, Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);

        int startIndex = vh.currentVertCount;

        vh.AddVert(start - normal, color, Vector2.zero);
        vh.AddVert(start + normal, color, Vector2.zero);
        vh.AddVert(end + normal, color, Vector2.zero);
        vh.AddVert(end - normal, color, Vector2.zero);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }
}
