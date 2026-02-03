using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float damage = 10f;

    [SerializeField]
    private float lifetime = 2f;

    [SerializeField]
    private int pierce = 1;

    private Vector2 _direction;
    private int _remainingHits;
    private Collider2D _collider;
    private readonly System.Collections.Generic.HashSet<Health> _hitTargets = new System.Collections.Generic.HashSet<Health>();

    public void Initialize(Vector2 direction, float speedValue, float damageValue, float lifetimeValue, int pierceCount)
    {
        _direction = direction.normalized;
        speed = speedValue;
        damage = damageValue;
        lifetime = lifetimeValue;
        pierce = Mathf.Max(0, pierceCount);
        _remainingHits = pierce + 1;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _remainingHits = pierce + 1;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_remainingHits <= 0)
        {
            return;
        }

        if (other.GetComponent<PlayerController>() != null)
        {
            return;
        }

        var health = other.GetComponent<Health>();
        if (health == null)
        {
            return;
        }

        if (_hitTargets.Contains(health))
        {
            return;
        }

        _hitTargets.Add(health);
        health.Damage(damage);

        _remainingHits -= 1;
        if (_remainingHits <= 0)
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }
            Destroy(gameObject);
        }
    }
}
