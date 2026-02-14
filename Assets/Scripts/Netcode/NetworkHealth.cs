using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class NetworkHealth : NetworkBehaviour
{
    private readonly NetworkVariable<float> _current = new NetworkVariable<float>(100f);
    private readonly NetworkVariable<float> _max = new NetworkVariable<float>(100f);
    private Health _health;
    private float _lastCurrent;
    private float _lastMax;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    public override void OnNetworkSpawn()
    {
        if (_health == null)
        {
            _health = GetComponent<Health>();
        }

        if (IsServer)
        {
            SyncFromHealth(true);
        }

        ApplyToHealth();
        _current.OnValueChanged += OnCurrentChanged;
        _max.OnValueChanged += OnMaxChanged;
    }

    public override void OnNetworkDespawn()
    {
        _current.OnValueChanged -= OnCurrentChanged;
        _max.OnValueChanged -= OnMaxChanged;
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        SyncFromHealth(false);
    }

    private void SyncFromHealth(bool force)
    {
        if (_health == null)
        {
            return;
        }

        float current = _health.CurrentHealth;
        float max = _health.MaxHealth;

        if (force || !Mathf.Approximately(current, _lastCurrent))
        {
            _current.Value = current;
            _lastCurrent = current;
        }

        if (force || !Mathf.Approximately(max, _lastMax))
        {
            _max.Value = max;
            _lastMax = max;
        }
    }

    private void OnCurrentChanged(float previous, float next)
    {
        if (IsServer)
        {
            return;
        }

        ApplyToHealth();
    }

    private void OnMaxChanged(float previous, float next)
    {
        if (IsServer)
        {
            return;
        }

        ApplyToHealth();
    }

    private void ApplyToHealth()
    {
        if (_health == null)
        {
            return;
        }

        float prev = _health.CurrentHealth;
        _health.SetMaxHealth(_max.Value, false);
        bool invokeDamage = _current.Value < prev;
        _health.SetCurrentHealth(_current.Value, true, invokeDamage);
    }
}
