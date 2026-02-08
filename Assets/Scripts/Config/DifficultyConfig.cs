using UnityEngine;

[CreateAssetMenu(menuName = "Game/Difficulty Config", fileName = "DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    public string difficultyName = "Normal";

    [Header("Spawn")]
    public float spawnIntervalMultiplier = 1f;
    public float minSpawnIntervalMultiplier = 1f;
    public float spawnIntervalDecayMultiplier = 1f;
    public float maxEnemiesMultiplier = 1f;
    public float maxEnemiesPerMinuteMultiplier = 1f;

    [Header("Enemy Base Stats")]
    public float enemyHealthMultiplier = 1f;
    public float enemyDamageMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;
    public float enemyXpMultiplier = 1f;

    [Header("Enemy Per Level")]
    public float enemyHealthPerLevelMultiplier = 1f;
    public float enemyDamagePerLevelMultiplier = 1f;
    public float enemySpeedPerLevelMultiplier = 1f;
    public float enemyXpPerLevelMultiplier = 1f;

    [Header("Rewards")]
    public float coinDropChanceMultiplier = 1f;
    public float xpGainMultiplier = 1f;
}
