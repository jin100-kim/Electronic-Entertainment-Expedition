using System.Collections.Generic;
using UnityEngine;

public class DroneProjectile : MonoBehaviour
{
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

    public void Initialize(Transform owner, float radius, float speed, float damageValue, float lifetimeValue, float startAngle)
    {
        _owner = owner;
        orbitRadius = Mathf.Max(0.1f, radius);
        angularSpeed = speed;
        damage = damageValue;
        lifetime = Mathf.Max(0.2f, lifetimeValue);
        _angle = startAngle;
    }

    private void Update()
    {
        if (_owner == null)
        {
            Destroy(gameObject);
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        _angle += angularSpeed * Time.deltaTime;
        Vector2 offset = new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle)) * orbitRadius;
        transform.position = _owner.position + (Vector3)offset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
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
    }
}
