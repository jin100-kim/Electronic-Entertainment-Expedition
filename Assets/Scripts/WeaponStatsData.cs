using UnityEngine;

[System.Serializable]
public class WeaponStatsData
{
    public string displayName = "무기";
    public int level = 0;
    public bool unlocked = false;
    public float damageMult = 1f;
    public float fireRateMult = 1f;
    public float rangeMult = 1f;
    public int bonusProjectiles = 0;
}
