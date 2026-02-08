using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stage Config", fileName = "StageConfig")]
public class StageConfig : ScriptableObject
{
    public string stageName = "스테이지 1";

    [Header("Stage Clear")]
    public float timeLimitSeconds = 0f;
    public int killTarget = 0;

    [Header("Map")]
    public Vector2 mapHalfSize = new Vector2(24f, 24f);
    public Vector3 localSpawnPosition = Vector3.zero;

    [Header("Spawn")]
    public float spawnInterval = 2f;
    public int maxEnemies = 20;
    public float spawnRadius = 8f;
    public float minSpawnInterval = 0.4f;
    public float spawnIntervalDecayPerSec = 0.01f;
    public int maxEnemiesPerMinute = 10;
}
