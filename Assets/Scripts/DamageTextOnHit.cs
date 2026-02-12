using UnityEngine;

public class DamageTextOnHit : MonoBehaviour
{
    [SerializeField]
    private Vector3 worldOffset = new Vector3(0f, 0.4f, 0f);

    private Health _health;
    private float _offsetY;

    private void Awake()
    {
        _health = GetComponent<Health>();
        CacheOffset();
    }

    private void OnEnable()
    {
        if (_health == null)
        {
            _health = GetComponent<Health>();
        }

        if (_health != null)
        {
            _health.OnDamaged += OnDamaged;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDamaged -= OnDamaged;
        }
    }

    private void CacheOffset()
    {
        float y = 0f;
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            y = col.bounds.extents.y;
        }
        else
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                y = renderer.bounds.extents.y;
            }
        }

        _offsetY = Mathf.Max(0.1f, y);
    }

    private void OnDamaged(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        Vector3 pos = transform.position + worldOffset + new Vector3(0f, _offsetY * 0.5f, 0f);
        DamageText.Spawn(pos, amount);
    }
}
