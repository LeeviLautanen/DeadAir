using UnityEngine.UI;
using UnityEngine;

public class UpgradeNodeUILine : Graphic
{
    public Vector2 a;
    public Vector2 b;
    public float thickness = 2f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        var dir = (b - a).normalized;
        var normal = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

        var v1 = a - normal;
        var v2 = a + normal;
        var v3 = b + normal;
        var v4 = b - normal;

        vh.AddVert(v1, color, Vector2.zero);
        vh.AddVert(v2, color, Vector2.zero);
        vh.AddVert(v3, color, Vector2.zero);
        vh.AddVert(v4, color, Vector2.zero);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}