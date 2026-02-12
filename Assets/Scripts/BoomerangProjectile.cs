using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BoomerangProjectile : MonoBehaviour
{
    public static readonly List<BoomerangProjectile> Active = new List<BoomerangProjectile>();
    private const float PierceDamageMultiplierPerHit = 0.8f;
    private const float MinPierceDamage = 1f;

    [SerializeField]
    private float speed = 8f;

    [SerializeField]
    private float damage = 8f;
    private float _baseDamage = 8f;

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

    [SerializeField]
    private float spinSpeed = 1440f;

    private Vector2 _direction;
    private Transform _owner;
    private float _elapsed;
    private int _remainingHits;
    private int _pierceHitCount;
    private bool _infinitePierce;
    private Collider2D _collider;
    private readonly HashSet<Health> _hitTargets = new HashSet<Health>();
    private bool _returning;
    private System.Action<BoomerangProjectile> _release;
    private float _hitStunDuration;
    private float _knockbackDistance;
    private ElementType _elementFirst = ElementType.None;
    private ElementType _elementSecond = ElementType.None;
    private ElementType _elementThird = ElementType.None;
    private int _elementCount;

    public void Initialize(Transform owner, Vector2 direction, float speedValue, float damageValue, float lifetimeValue, int pierceCount)
    {
        ResetState();
        _owner = owner;
        _direction = direction.normalized;
        speed = speedValue;
        damage = damageValue;
        _baseDamage = damageValue;
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
        _baseDamage = damage;
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
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            ApplySpin();
            return;
        }

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
                    Despawn();
                    return;
                }

                speed = Mathf.Min(speed + returnAcceleration * Time.deltaTime, returnMaxSpeed);
                _direction = toOwner.normalized;
            }
        }

        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
        ApplySpin();

        if (_elapsed >= lifetime)
        {
            Despawn();
        }
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

    public void SetRelease(System.Action<BoomerangProjectile> release)
    {
        _release = release;
    }

    public void SetHitReaction(float knockbackDistance, float hitStunDuration)
    {
        _knockbackDistance = Mathf.Max(0f, knockbackDistance);
        _hitStunDuration = Mathf.Max(0f, hitStunDuration);
    }

    public void SetElements(ElementType first, ElementType second, ElementType third, int count)
    {
        _elementCount = Mathf.Clamp(count, 0, 3);
        _elementFirst = _elementCount >= 1 ? first : ElementType.None;
        _elementSecond = _elementCount >= 2 ? second : ElementType.None;
        _elementThird = _elementCount >= 3 ? third : ElementType.None;
    }

    private void BeginReturn()
    {
        _returning = true;
        _hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        if (_remainingHits <= 0)
        {
            return;
        }

        if (_owner != null && other.transform == _owner)
        {
            if (_returning)
            {
                Despawn();
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
        float hitDamage = GetPierceScaledDamage();
        health.Damage(hitDamage);
        _pierceHitCount += 1;

        var status = other.GetComponent<ElementStatus>();
        if (status != null)
        {
            ElementSystem.ApplyElementsOnHit(_elementFirst, _elementSecond, _elementThird, _elementCount, status);
        }

        if (_hitStunDuration > 0f || _knockbackDistance > 0f)
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                Vector2 dir = (Vector2)(other.transform.position - transform.position);
                if (dir.sqrMagnitude < 0.0001f)
                {
                    dir = _direction;
                }
                enemy.ApplyHitReaction(dir, _knockbackDistance, _hitStunDuration);
            }
        }

        if (!_infinitePierce)
        {
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
    }

    private void Despawn()
    {
        if (_release != null)
        {
            _release(this);
        }
        else if (NetworkSession.IsActive)
        {
            var netObj = GetComponent<NetworkObject>();
            if (NetworkSession.IsServer && netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetState()
    {
        _elapsed = 0f;
        _returning = false;
        _hitTargets.Clear();
        _pierceHitCount = 0;
        _hitStunDuration = 0f;
        _knockbackDistance = 0f;
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

    private void ApplySpin()
    {
        if (Mathf.Abs(spinSpeed) > 0.01f)
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }
    }

    private float GetPierceScaledDamage()
    {
        float scaled = _baseDamage * Mathf.Pow(PierceDamageMultiplierPerHit, _pierceHitCount);
        return Mathf.Max(MinPierceDamage, scaled);
    }
}
