using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<Projectile> Active = new System.Collections.Generic.List<Projectile>();

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
    private float _spinSpeed;
    private bool _useOrbit;
    private Vector2 _orbitCenter;
    private float _orbitAngle;
    private float _orbitRadius;
    private float _orbitAngularSpeed;
    private float _orbitRadialSpeed;
    private bool _applySlow;
    private float _slowMultiplier = 1f;
    private float _slowDuration;
    private System.Action<Projectile> _release;

    public void Initialize(Vector2 direction, float speedValue, float damageValue, float lifetimeValue, int pierceCount, float spinSpeed = 0f)
    {
        ResetState();
        _direction = direction.normalized;
        speed = speedValue;
        damage = damageValue;
        lifetime = lifetimeValue;
        pierce = Mathf.Max(0, pierceCount);
        _remainingHits = pierce + 1;
        _spinSpeed = spinSpeed;
    }

    public void InitializeOrbit(Vector2 center, Vector2 direction, float radialSpeed, float angularSpeed, float damageValue, float lifetimeValue, int pierceCount, float spinSpeed = 0f)
    {
        ResetState();
        _useOrbit = true;
        _orbitCenter = center;
        _direction = direction.normalized;
        _orbitAngle = Mathf.Atan2(_direction.y, _direction.x);
        _orbitRadius = 0.2f;
        _orbitRadialSpeed = radialSpeed;
        _orbitAngularSpeed = angularSpeed;

        speed = radialSpeed;
        damage = damageValue;
        lifetime = lifetimeValue;
        pierce = Mathf.Max(0, pierceCount);
        _remainingHits = pierce + 1;
        _spinSpeed = spinSpeed;
    }

    public void SetSlowEffect(float multiplier, float duration)
    {
        _applySlow = duration > 0f;
        _slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        _slowDuration = Mathf.Max(0f, duration);
    }

    public void SetRelease(System.Action<Projectile> release)
    {
        _release = release;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _remainingHits = pierce + 1;
    }

    private void OnEnable()
    {
        if (!Active.Contains(this))
        {
            Active.Add(this);
        }
    }

    private void OnDisable()
    {
        Active.Remove(this);
    }

    private void Update()
    {
        if (_useOrbit)
        {
            _orbitRadius += _orbitRadialSpeed * Time.deltaTime;
            _orbitAngle += _orbitAngularSpeed * Time.deltaTime;
            Vector2 offset = new Vector2(Mathf.Cos(_orbitAngle), Mathf.Sin(_orbitAngle)) * _orbitRadius;
            transform.position = _orbitCenter + offset;
        }
        else
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);
        }
        if (Mathf.Abs(_spinSpeed) > 0.01f)
        {
            transform.Rotate(0f, 0f, _spinSpeed * Time.deltaTime);
        }
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Despawn();
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

        if (_applySlow)
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.ApplySlow(_slowMultiplier, _slowDuration);
            }
        }

        _remainingHits -= 1;
        if (_remainingHits <= 0)
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }
            Despawn();
        }
    }

    private void Despawn()
    {
        if (_release != null)
        {
            _release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetState()
    {
        _useOrbit = false;
        _applySlow = false;
        _hitTargets.Clear();
        _slowMultiplier = 1f;
        _slowDuration = 0f;
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }
}
