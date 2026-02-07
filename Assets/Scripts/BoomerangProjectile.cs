using System.Collections.Generic;
using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 8f;

    [SerializeField]
    private float damage = 8f;

    [SerializeField]
    private float lifetime = 2f;

    [SerializeField]
    private int pierce = 0;

    [SerializeField]
    private float returnAcceleration = 18f;

    [SerializeField]
    private float returnMaxSpeed = 18f;

    [SerializeField]
    private float catchDistance = 0.35f;

    private Vector2 _direction;
    private Transform _owner;
    private float _elapsed;
    private int _remainingHits;
    private bool _infinitePierce;
    private Collider2D _collider;
    private readonly HashSet<Health> _hitTargets = new HashSet<Health>();
    private bool _returning;

    public void Initialize(Transform owner, Vector2 direction, float speedValue, float damageValue, float lifetimeValue, int pierceCount)
    {
        _owner = owner;
        _direction = direction.normalized;
        speed = speedValue;
        damage = damageValue;
        lifetime = lifetimeValue;
        pierce = Mathf.Max(0, pierceCount);
        if (pierce >= 9999)
        {
            _infinitePierce = true;
            _remainingHits = int.MaxValue;
        }
        else
        {
            _remainingHits = pierce + 1;
        }
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (pierce >= 9999)
        {
            _infinitePierce = true;
            _remainingHits = int.MaxValue;
        }
        else
        {
            _remainingHits = pierce + 1;
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float half = lifetime * 0.5f;
        if (!_returning && _elapsed >= half)
        {
            BeginReturn();
        }

        if (_returning)
        {
            if (_owner != null)
            {
                Vector2 toOwner = (Vector2)_owner.position - (Vector2)transform.position;
                if (toOwner.sqrMagnitude <= catchDistance * catchDistance)
                {
                    Destroy(gameObject);
                    return;
                }

                speed = Mathf.Min(speed + returnAcceleration * Time.deltaTime, returnMaxSpeed);
                _direction = toOwner.normalized;
            }
        }

        transform.position += (Vector3)(_direction * speed * Time.deltaTime);

        if (_elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void BeginReturn()
    {
        _returning = true;
        _hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_remainingHits <= 0)
        {
            return;
        }

        if (_owner != null && other.transform == _owner)
        {
            if (_returning)
            {
                Destroy(gameObject);
            }
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

        if (!_infinitePierce)
        {
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
}
