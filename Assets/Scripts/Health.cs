using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;
    public float RegenPerSecond { get; private set; }

    public System.Action<float> OnDamaged;
    public System.Action OnDied;

    private void Awake()
    {
        if (CurrentHealth <= 0f)
        {
            CurrentHealth = maxHealth;
        }
    }

    private void Update()
    {
        if (IsDead || RegenPerSecond <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + RegenPerSecond * Time.deltaTime);
    }

    public void Damage(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnDamaged?.Invoke(amount);

        if (IsDead)
        {
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
    }

    public void SetCurrentHealth(float value, bool clampToMax = true, bool invokeDamagedEvent = false)
    {
        float prev = CurrentHealth;
        float next = clampToMax ? Mathf.Clamp(value, 0f, maxHealth) : value;
        CurrentHealth = next;

        if (invokeDamagedEvent && next < prev)
        {
            OnDamaged?.Invoke(prev - next);
        }

        if (prev > 0f && next <= 0f)
        {
            OnDied?.Invoke();
        }
    }

    public void AddMaxHealth(float amount, bool healToFull = false)
    {
        if (amount <= 0f)
        {
            return;
        }

        maxHealth += amount;
        if (healToFull)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        }
    }

    public void SetMaxHealth(float value, bool healToFull = false)
    {
        float newMax = Mathf.Max(1f, value);
        maxHealth = newMax;
        if (healToFull)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        }
    }

    public void SetRegenPerSecond(float value)
    {
        RegenPerSecond = Mathf.Max(0f, value);
    }
}
