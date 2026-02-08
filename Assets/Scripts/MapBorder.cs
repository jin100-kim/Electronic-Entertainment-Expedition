using UnityEngine;

public class MapBorder : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private Color color = new Color(1f, 1f, 1f, 0.6f);

    [SerializeField]
    private float width = 0.05f;

    private LineRenderer _line;
    private bool _settingsApplied;

    private void Awake()
    {
        ApplySettings();
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
        _line.sortingOrder = sortingOrder;
    }

    [SerializeField]
    private int sortingOrder = 100;

    private void ApplySettings()
    {
        if (_settingsApplied)
        {
            return;
        }

        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.mapBorder;

        color = settings.color;
        width = settings.width;
        sortingOrder = settings.sortingOrder;
        _settingsApplied = true;
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
