using UnityEngine;

[ExecuteAlways]
public class ColliderGizmos : MonoBehaviour
{
    public static bool GlobalEnabled { get; private set; }

    [SerializeField]
    private Color gizmoColor = new Color(1f, 0.4f, 0.4f, 0.85f);

    [SerializeField]
    private bool drawOnSelectedOnly = false;

    [SerializeField]
    private bool alwaysDraw = false;

    private void OnDrawGizmos()
    {
        if (!drawOnSelectedOnly)
        {
            DrawGizmos();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (drawOnSelectedOnly)
        {
            DrawGizmos();
        }
    }

    private void DrawGizmos()
    {
        if (!GlobalEnabled && !alwaysDraw)
        {
            return;
        }

        var colliders = GetComponents<Collider2D>();
        if (colliders == null || colliders.Length == 0)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        for (int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];
            if (col == null || !col.enabled)
            {
                continue;
            }

            if (col is CircleCollider2D circle)
            {
                DrawCircle(circle);
            }
            else if (col is BoxCollider2D box)
            {
                DrawBox(box);
            }
            else if (col is CapsuleCollider2D capsule)
            {
                DrawCapsule(capsule);
            }
            else if (col is PolygonCollider2D poly)
            {
                DrawPolygon(poly);
            }
        }
    }

    private static void DrawCircle(CircleCollider2D circle)
    {
        var t = circle.transform;
        Vector3 center = t.TransformPoint(circle.offset);
        Gizmos.matrix = Matrix4x4.TRS(center, t.rotation, t.lossyScale);
        Gizmos.DrawWireSphere(Vector3.zero, circle.radius);
    }

    private static void DrawBox(BoxCollider2D box)
    {
        var t = box.transform;
        Vector3 center = t.TransformPoint(box.offset);
        Gizmos.matrix = Matrix4x4.TRS(center, t.rotation, t.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, box.size);
    }

    private static void DrawCapsule(CapsuleCollider2D capsule)
    {
        var t = capsule.transform;
        Vector3 center = t.TransformPoint(capsule.offset);
        Gizmos.matrix = Matrix4x4.TRS(center, t.rotation, t.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, capsule.size);
    }

    private static void DrawPolygon(PolygonCollider2D poly)
    {
        var t = poly.transform;
        Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        for (int p = 0; p < poly.pathCount; p++)
        {
            var points = poly.GetPath(p);
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 a = points[i];
                Vector3 b = points[(i + 1) % points.Length];
                Gizmos.DrawLine(a, b);
            }
        }
    }

    public static void SetGlobalEnabled(bool enabled)
    {
        GlobalEnabled = enabled;
        if (enabled)
        {
            EnsureAllCollidersTracked();
        }
    }

    public static void EnsureAllCollidersTracked()
    {
        var colliders = Object.FindObjectsOfType<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];
            if (col == null)
            {
                continue;
            }

            if (col.GetComponent<ColliderGizmos>() == null)
            {
                col.gameObject.AddComponent<ColliderGizmos>();
            }
        }
    }
}
