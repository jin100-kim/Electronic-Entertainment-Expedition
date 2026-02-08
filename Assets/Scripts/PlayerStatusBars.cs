using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStatusBars : MonoBehaviour
{
    [SerializeField]
    private float barWidth = 0.9f;

    [SerializeField]
    private float barHeight = 0.12f;

    [SerializeField]
    private float spacing = 0.12f;

    [SerializeField]
    private Vector3 offset = new Vector3(0f, 0.65f, 0f);

    [SerializeField]
    private Color hpColor = new Color(0.2f, 0.9f, 0.3f, 1f);

    private Health _health;
    private Transform _hpFill;
    private SpriteRenderer _hpFillRenderer;
    private readonly System.Collections.Generic.List<SpriteRenderer> _renderers = new System.Collections.Generic.List<SpriteRenderer>();
    private bool _isVisible = true;

    private void Awake()
    {
        _health = GetComponent<Health>();
        CreateBars();
    }

    private void LateUpdate()
    {
        bool show = true;
        var session = GameSession.Instance;
        if (session != null && !session.IsGameplayActive)
        {
            show = false;
        }

        if (show != _isVisible)
        {
            SetRenderersVisible(show);
            _isVisible = show;
        }

        if (!show)
        {
            return;
        }

        if (_health == null || _hpFill == null)
        {
            return;
        }

        float hpRatio = _health.MaxHealth <= 0f ? 0f : _health.CurrentHealth / _health.MaxHealth;
        SetFill(_hpFill, hpRatio, barWidth, barHeight);
    }

    private void CreateBars()
    {
        var root = new GameObject("StatusBars");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = offset;

        _hpFill = CreateBar(root.transform, "HPBar", hpColor, 20, Vector3.zero, out _hpFillRenderer);
    }

    private Transform CreateBar(Transform parent, string name, Color fillColor, int sortingOrder, Vector3 localOffset, out SpriteRenderer fillRenderer)
    {
        var barRoot = new GameObject(name);
        barRoot.transform.SetParent(parent, false);
        barRoot.transform.localPosition = localOffset;

        var bg = new GameObject("BG");
        bg.transform.SetParent(barRoot.transform, false);
        var bgRenderer = bg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateSolidSprite();
        bgRenderer.color = new Color(0f, 0f, 0f, 0.6f);
        bgRenderer.sortingOrder = sortingOrder;
        bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        _renderers.Add(bgRenderer);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(barRoot.transform, false);
        fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateSolidSprite();
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = sortingOrder + 1;
        fill.transform.localPosition = new Vector3(-barWidth * 0.5f, 0f, 0f);
        fill.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        _renderers.Add(fillRenderer);

        var pivot = fill.AddComponent<BarPivot>();
        pivot.SetLeftAnchored(barWidth);

        return fill.transform;
    }

    private void SetRenderersVisible(bool visible)
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_renderers[i] != null)
            {
                _renderers[i].enabled = visible;
            }
        }
    }

    private static void SetFill(Transform fill, float ratio, float width, float height)
    {
        if (fill == null)
        {
            return;
        }

        ratio = Mathf.Clamp01(ratio);
        var scale = fill.localScale;
        scale.x = width * ratio;
        scale.y = height;
        fill.localScale = scale;
    }

    private static Sprite CreateSolidSprite()
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private class BarPivot : MonoBehaviour
    {
        private float _width;

        public void SetLeftAnchored(float width)
        {
            _width = width;
        }

        private void LateUpdate()
        {
            var t = transform;
            var scale = t.localScale;
            float ratio = _width <= 0f ? 0f : scale.x / _width;
            t.localPosition = new Vector3(-_width * 0.5f + (_width * 0.5f * ratio), 0f, 0f);
        }
    }
}
