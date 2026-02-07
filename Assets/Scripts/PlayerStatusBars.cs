using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStatusBars : MonoBehaviour
{
    [SerializeField]
    private float width = 0.9f;

    [SerializeField]
    private float height = 0.12f;

    [SerializeField]
    private float spacing = 0.14f;

    [SerializeField]
    private Vector3 offset = new Vector3(0f, 0.55f, 0f);

    [SerializeField]
    private Color hpColor = new Color(0.2f, 0.9f, 0.3f, 1f);

    [SerializeField]
    private Color xpColor = new Color(0.3f, 0.6f, 1f, 1f);

    private Health _health;
    private Experience _xp;
    private Transform _hpFill;
    private Transform _xpFill;

    private void Awake()
    {
        _health = GetComponent<Health>();
        CreateBars();
    }

    private void LateUpdate()
    {
        if (_health == null || _hpFill == null)
        {
            return;
        }

        float hpRatio = _health.MaxHealth <= 0f ? 0f : _health.CurrentHealth / _health.MaxHealth;
        SetFill(_hpFill, hpRatio);

        if (_xp == null)
        {
            _xp = GetComponent<Experience>();
        }

        if (_xpFill != null)
        {
            float xpRatio = _xp != null && _xp.XpToNext > 0f ? _xp.CurrentXp / _xp.XpToNext : 0f;
            SetFill(_xpFill, xpRatio);
        }
    }

    private void CreateBars()
    {
        var root = new GameObject("StatusBars");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = offset;

        _hpFill = CreateBar(root.transform, "HPBar", hpColor, 20, Vector3.zero);
        _xpFill = CreateBar(root.transform, "XPBar", xpColor, 22, new Vector3(0f, -spacing, 0f));
    }

    private Transform CreateBar(Transform parent, string name, Color fillColor, int sortingOrder, Vector3 localOffset)
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
        bg.transform.localScale = new Vector3(width, height, 1f);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(barRoot.transform, false);
        var fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateSolidSprite();
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = sortingOrder + 1;
        fill.transform.localPosition = new Vector3(-width * 0.5f, 0f, 0f);
        fill.transform.localScale = new Vector3(width, height, 1f);

        var pivot = fill.AddComponent<BarPivot>();
        pivot.SetLeftAnchored(width);

        return fill.transform;
    }

    private static void SetFill(Transform fill, float ratio)
    {
        if (fill == null)
        {
            return;
        }

        ratio = Mathf.Clamp01(ratio);
        var scale = fill.localScale;
        scale.x = ratio;
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
            t.localPosition = new Vector3(-_width * 0.5f + (_width * 0.5f * scale.x), 0f, 0f);
        }
    }
}
