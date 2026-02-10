using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DroneProjectile : MonoBehaviour
{
    public static readonly List<DroneProjectile> Active = new List<DroneProjectile>();

    [SerializeField]
    private float damage = 6f;

    [SerializeField]
    private float orbitRadius = 1.6f;

    [SerializeField]
    private float angularSpeed = 3f;

    [SerializeField]
    private float lifetime = 6f;

    [SerializeField]
    private float hitCooldown = 0.3f;

    private Transform _owner;
    private float _angle;
    private float _timer;
    private readonly Dictionary<Health, float> _hitTimes = new Dictionary<Health, float>();
    private System.Action<DroneProjectile> _release;
    private ElementType _elementFirst = ElementType.None;
    private ElementType _elementSecond = ElementType.None;
    private ElementType _elementThird = ElementType.None;
    private int _elementCount;

    public void Initialize(Transform owner, float radius, float speed, float damageValue, float lifetimeValue, float startAngle)
    {
        ResetState();
        _owner = owner;
        orbitRadius = Mathf.Max(0.1f, radius);
        angularSpeed = speed;
        damage = damageValue;
        lifetime = Mathf.Max(0.2f, lifetimeValue);
        _angle = startAngle;
    }

    public void SetRelease(System.Action<DroneProjectile> release)
    {
        _release = release;
    }

    public void SetElements(ElementType first, ElementType second, ElementType third, int count)
    {
        _elementCount = Mathf.Clamp(count, 0, 3);
        _elementFirst = _elementCount >= 1 ? first : ElementType.None;
        _elementSecond = _elementCount >= 2 ? second : ElementType.None;
        _elementThird = _elementCount >= 3 ? third : ElementType.None;
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
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        if (_owner == null)
        {
            Despawn();
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            Despawn();
            return;
        }

        _angle += angularSpeed * Time.deltaTime;
        Vector2 offset = new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle)) * orbitRadius;
        transform.position = _owner.position + (Vector3)offset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        TryHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return;
        }

        TryHit(other);
    }

    private void TryHit(Collider2D other)
    {
        if (other == null)
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

        float now = Time.time;
        if (_hitTimes.TryGetValue(health, out var last) && now - last < hitCooldown)
        {
            return;
        }

        _hitTimes[health] = now;
        health.Damage(damage);

        var status = other.GetComponent<ElementStatus>();
        if (status != null)
        {
            ElementSystem.ApplyElementsOnHit(_elementFirst, _elementSecond, _elementThird, _elementCount, status);
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
        _timer = 0f;
        _hitTimes.Clear();
    }
}
