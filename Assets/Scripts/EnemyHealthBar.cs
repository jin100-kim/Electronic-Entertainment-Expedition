using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField]
    private float width = 0.8f;

    [SerializeField]
    private float height = 0.12f;

    [SerializeField]
    private Vector3 offset = new Vector3(0f, 0.45f, 0f);

    private Health _health;
    private Transform _fill;

    private void Awake()
    {
        _health = GetComponent<Health>();
        CreateBar();
    }

    private void LateUpdate()
    {
        if (_health == null || _fill == null)
        {
            return;
        }

        float ratio = _health.MaxHealth <= 0f ? 0f : _health.CurrentHealth / _health.MaxHealth;
        ratio = Mathf.Clamp01(ratio);

        var scale = _fill.localScale;
        scale.x = ratio;
        _fill.localScale = scale;
    }

    private void CreateBar()
    {
        var barRoot = new GameObject("HealthBar");
        barRoot.transform.SetParent(transform, false);
        barRoot.transform.localPosition = offset;

        var bg = new GameObject("BG");
        bg.transform.SetParent(barRoot.transform, false);
        var bgRenderer = bg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateSolidSprite();
        bgRenderer.color = new Color(0f, 0f, 0f, 0.6f);
        bgRenderer.sortingOrder = 10;
        bg.transform.localScale = new Vector3(width, height, 1f);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(barRoot.transform, false);
        var fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateSolidSprite();
        fillRenderer.color = new Color(0.2f, 0.9f, 0.3f, 1f);
        fillRenderer.sortingOrder = 11;
        fill.transform.localPosition = new Vector3(-width * 0.5f, 0f, 0f);
        fill.transform.localScale = new Vector3(width, height, 1f);
        _fill = fill.transform;

        // Keep left edge anchored by scaling from left
        var pivot = fill.AddComponent<BarPivot>();
        pivot.SetLeftAnchored(width);
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
            // keep left edge anchored as X scale changes
            var t = transform;
            var scale = t.localScale;
            t.localPosition = new Vector3(-_width * 0.5f + (_width * 0.5f * scale.x), 0f, 0f);
        }
    }
}
