using UnityEngine;

public class Experience : MonoBehaviour
{
    [SerializeField]
    private int level = 1;

    [SerializeField]
    private float currentXp = 0f;

    [SerializeField]
    private float xpToNext = 8f;

    [SerializeField]
    private float xpGrowth = 4f;

    [SerializeField]
    private float xpMultiplier = 1.5f;

    [Header("Magnet")]
    [SerializeField]
    private float baseMagnetRange = 1.5f;

    [SerializeField]
    private float baseMagnetSpeed = 1.5f;

    private float _magnetRangeMult = 1f;
    private float _magnetSpeedMult = 1f;

    public int Level => level;
    public float CurrentXp => currentXp;
    public float XpToNext => xpToNext;
    public float MagnetRange { get; private set; }
    public float MagnetSpeed { get; private set; }

    public System.Action<int> OnLevelUp;

    private void Awake()
    {
        RecalculateMagnet();
    }

    public void AddXp(float amount)
    {
        if (amount <= 0)
        {
            return;
        }

        float finalAmount = Mathf.Max(0.01f, amount * xpMultiplier);
        currentXp += finalAmount;
        while (currentXp >= xpToNext)
        {
            currentXp -= xpToNext;
            level++;
            xpToNext += xpGrowth;
            OnLevelUp?.Invoke(level);
        }
    }

    public void SetXpMultiplier(float value)
    {
        xpMultiplier = Mathf.Max(0.1f, value);
    }

    public void SetMagnetMultiplier(float rangeMult, float speedMult)
    {
        _magnetRangeMult = Mathf.Max(0.1f, rangeMult);
        _magnetSpeedMult = Mathf.Max(0.1f, speedMult);
        RecalculateMagnet();
    }

    private void RecalculateMagnet()
    {
        MagnetRange = Mathf.Max(0.1f, baseMagnetRange * _magnetRangeMult);
        MagnetSpeed = Mathf.Max(0.1f, baseMagnetSpeed * _magnetSpeedMult);
    }
}
