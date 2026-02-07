using UnityEngine;

public class MapBorder : MonoBehaviour
{
    [SerializeField]
    private Color color = new Color(1f, 1f, 1f, 0.6f);

    [SerializeField]
    private float width = 0.05f;

    private LineRenderer _line;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        if (_line == null)
        {
            _line = gameObject.AddComponent<LineRenderer>();
        }

        _line.useWorldSpace = true;
        _line.loop = true;
        _line.startWidth = width;
        _line.endWidth = width;
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = color;
        _line.endColor = color;
        _line.sortingOrder = 100;
    }

    public void SetBounds(Vector2 halfSize)
    {
        var points = new Vector3[4];
        points[0] = new Vector3(-halfSize.x, -halfSize.y, 0f);
        points[1] = new Vector3(halfSize.x, -halfSize.y, 0f);
        points[2] = new Vector3(halfSize.x, halfSize.y, 0f);
        points[3] = new Vector3(-halfSize.x, halfSize.y, 0f);

        _line.positionCount = points.Length;
        _line.SetPositions(points);
    }
}
