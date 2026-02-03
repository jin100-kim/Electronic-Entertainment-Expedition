using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Start Mode")]
    [SerializeField]
    private bool autoStartLocal = false;

    [SerializeField]
    private bool showNetworkUI = true;

    [Header("Spawn")]
    [SerializeField]
    private float spawnInterval = 2f;

    [SerializeField]
    private int maxEnemies = 20;

    [SerializeField]
    private float spawnRadius = 8f;

    [Header("Difficulty")]
    [SerializeField]
    private float minSpawnInterval = 0.4f;

    [SerializeField]
    private float spawnIntervalDecayPerSec = 0.01f;

    [SerializeField]
    private int maxEnemiesPerMinute = 10;

    [Header("Player")]
    [SerializeField]
    private Vector3 localSpawnPosition = Vector3.zero;

    [Header("Map Bounds")]
    [SerializeField]
    private Vector2 mapHalfSize = new Vector2(12f, 12f);

    [Header("Upgrades")]
    [SerializeField]
    private float damageMult = 1f;

    [SerializeField]
    private float fireRateMult = 1f;

    [SerializeField]
    private float rangeMult = 1f;

    [SerializeField]
    private float sizeMult = 1f;

    [SerializeField]
    private float lifetimeMult = 1f;

    [SerializeField]
    private int projectileCount = 1;

    [SerializeField]
    private int projectilePierceBonus = 0;

    [SerializeField]
    private float weaponDamageMult = 1f;

    [SerializeField]
    private float straightWeaponDamageMult = 1f;

    [SerializeField]
    private float boomerangWeaponDamageMult = 1f;

    [SerializeField]
    private float novaWeaponDamageMult = 1f;

    [SerializeField]
    private float straightWeaponRangeMult = 1f;

    [SerializeField]
    private float boomerangWeaponRangeMult = 0.7f;

    [SerializeField]
    private float novaWeaponRangeMult = 0.5f;

    [SerializeField]
    private int straightWeaponLevel = 1;

    [SerializeField]
    private int boomerangWeaponLevel = 0;

    [SerializeField]
    private int novaWeaponLevel = 0;

    [SerializeField]
    private int novaBonusCount = 0;

    [SerializeField]
    private float moveSpeedMult = 1f;

    [SerializeField]
    private float xpGainMult = 1f;

    [SerializeField]
    private float regenPerSecond = 0f;

    [SerializeField]
    private bool boomerangUnlocked = false;

    [SerializeField]
    private bool novaUnlocked = false;

    public Vector2 MapHalfSize => mapHalfSize;

    public bool IsGameOver { get; private set; }
    public float ElapsedTime { get; private set; }
    public Health PlayerHealth { get; private set; }
    public Experience PlayerExperience { get; private set; }

    private EnemySpawner _spawner;
    private AutoAttack _attack;
    private PlayerController _player;
    private bool _gameStarted;

    private bool _choosingUpgrade;
    private readonly List<UpgradeOption> _options = new List<UpgradeOption>();
    private Vector2 _upgradeScroll;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!showNetworkUI)
        {
            DisableNetworkUI();
        }

        if (autoStartLocal)
        {
            StartLocalGame();
        }

        EnsureCameraFollow();
        EnsureMinimap();
        EnsureMapBorder();
    }

    private void Update()
    {
        if (!_gameStarted || IsGameOver)
        {
            return;
        }

        if (!_choosingUpgrade)
        {
            ElapsedTime += Time.deltaTime;
            ApplyDifficultyScaling();
        }
    }

    public static void StartNetworkGame()
    {
        if (Instance == null || Instance._gameStarted)
        {
            return;
        }

        Instance.StartNetworkSession();
    }

    private void StartNetworkSession()
    {
        _gameStarted = true;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            StartCoroutine(WaitForOwnerPlayer());
        }
        else
        {
            StartLocalGame();
        }
    }

    private IEnumerator WaitForOwnerPlayer()
    {
        PlayerController ownerPlayer = null;
        while (ownerPlayer == null)
        {
            var players = FindObjectsOfType<PlayerController>();
            foreach (var p in players)
            {
                if (p != null && p.IsOwner)
                {
                    ownerPlayer = p;
                    break;
                }
            }

            yield return null;
        }

        SetupPlayer(ownerPlayer);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            SetupSpawner(ownerPlayer.transform);
        }
    }

    private void StartLocalGame()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            player = CreateLocalPlayer(localSpawnPosition);
        }

        SetupPlayer(player);
        SetupSpawner(player.transform);
    }

    private void SetupPlayer(PlayerController player)
    {
        _player = player;
        _player.SetMoveSpeedMultiplier(moveSpeedMult);

        PlayerHealth = player.GetComponent<Health>();
        if (PlayerHealth != null)
        {
            PlayerHealth.ResetHealth();
            PlayerHealth.OnDied += OnPlayerDied;
        }

        PlayerExperience = player.GetComponent<Experience>();
        if (PlayerExperience == null)
        {
            PlayerExperience = player.gameObject.AddComponent<Experience>();
        }

        PlayerExperience.SetXpMultiplier(xpGainMult);
        PlayerExperience.OnLevelUp += OnLevelUp;

        _attack = player.GetComponent<AutoAttack>();
        if (_attack == null)
        {
            _attack = player.gameObject.AddComponent<AutoAttack>();
        }

        ApplyAttackStats();
        PlayerHealth?.SetRegenPerSecond(regenPerSecond);
    }

    private void SetupSpawner(Transform target)
    {
        _spawner = GetComponent<EnemySpawner>();
        if (_spawner == null)
        {
            _spawner = gameObject.AddComponent<EnemySpawner>();
        }

        _spawner.Target = target;
        _spawner.SpawnInterval = spawnInterval;
        _spawner.MaxEnemies = maxEnemies;
        _spawner.SpawnRadius = spawnRadius;
    }

    private PlayerController CreateLocalPlayer(Vector3 position)
    {
        var go = new GameObject("LocalPlayer");
        go.transform.position = position;

        if (go.GetComponent<NetworkObject>() == null)
        {
            go.AddComponent<NetworkObject>();
        }

        if (go.GetComponent<NetworkTransform>() == null)
        {
            go.AddComponent<NetworkTransform>();
        }

        var controller = go.AddComponent<PlayerController>();
        return controller;
    }

    private void OnPlayerDied()
    {
        IsGameOver = true;
        if (_spawner != null)
        {
            _spawner.enabled = false;
        }
    }

    private void OnLevelUp(int newLevel)
    {
        ShowUpgradeChoices();
    }

    private void ShowUpgradeChoices()
    {
        if (_choosingUpgrade)
        {
            return;
        }

        _options.Clear();

        _options.Add(new UpgradeOption("공격력 +25%", () => BuildPercentStatText("공격력", damageMult, damageMult * 1.25f), () => damageMult *= 1.25f));
        _options.Add(new UpgradeOption("공격속도 +20%", () => BuildPercentStatText("공격속도", fireRateMult, fireRateMult * 1.20f), () => fireRateMult *= 1.20f));
        _options.Add(new UpgradeOption("이동속도 +20%", () => BuildPercentStatText("이동속도", moveSpeedMult, moveSpeedMult * 1.20f), () => moveSpeedMult *= 1.20f));
        _options.Add(new UpgradeOption("체력 +40", () => BuildValueStatText("최대 체력", PlayerHealth != null ? PlayerHealth.MaxHealth : 0f, (PlayerHealth != null ? PlayerHealth.MaxHealth : 0f) + 40f), () => PlayerHealth?.AddMaxHealth(40f, true)));
        _options.Add(new UpgradeOption("체력재생 +1", () => BuildValueStatText("체력재생", regenPerSecond, regenPerSecond + 1f), () => regenPerSecond += 1f));
        _options.Add(new UpgradeOption("스킬 크기 +25%", () => BuildPercentStatText("스킬 크기", sizeMult, sizeMult * 1.25f), () => sizeMult *= 1.25f));
        _options.Add(new UpgradeOption("지속시간 +25%", () => BuildPercentStatText("지속시간", lifetimeMult, lifetimeMult * 1.25f), () => lifetimeMult *= 1.25f));
        _options.Add(new UpgradeOption("사거리 +10%", () => BuildPercentStatText("사거리", rangeMult, rangeMult * 1.10f), () => rangeMult *= 1.10f));
        _options.Add(new UpgradeOption("경험치 +35%", () => BuildPercentStatText("경험치 획득", xpGainMult, xpGainMult * 1.35f), () => xpGainMult *= 1.35f));
        if (!boomerangUnlocked)
        {
            _options.Add(new UpgradeOption("무기 획득: 부메랑", () => BuildWeaponAcquireText("부메랑", boomerangWeaponLevel), () => UnlockBoomerang()));
        }

        if (!novaUnlocked)
        {
            _options.Add(new UpgradeOption("무기 획득: 노바", () => BuildWeaponAcquireText("노바", novaWeaponLevel), () => UnlockNova()));
        }
        _options.Add(new UpgradeOption("기본 무기 강화", () => BuildStraightUpgradeText(), () => LevelUpStraightWeapon()));

        if (boomerangUnlocked)
        {
            _options.Add(new UpgradeOption("부메랑 강화", () => BuildBoomerangUpgradeText(), () => LevelUpBoomerangWeapon()));
        }

        if (novaUnlocked)
        {
            _options.Add(new UpgradeOption("노바 강화", () => BuildNovaUpgradeText(), () => LevelUpNovaWeapon()));
        }

        // random pick 3
        for (int i = _options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = _options[i];
            _options[i] = _options[j];
            _options[j] = temp;
        }

        if (_options.Count > 3)
        {
            _options.RemoveRange(3, _options.Count - 3);
        }

        _choosingUpgrade = true;
        Time.timeScale = 0f;
    }

    private void ApplyUpgrade(int index)
    {
        if (!_choosingUpgrade || index < 0 || index >= _options.Count)
        {
            return;
        }

        var opt = _options[index];
        opt.Apply?.Invoke();

        _choosingUpgrade = false;
        Time.timeScale = 1f;

        // apply updated stats
        _player?.SetMoveSpeedMultiplier(moveSpeedMult);
        PlayerExperience?.SetXpMultiplier(xpGainMult);
        PlayerHealth?.SetRegenPerSecond(regenPerSecond);
        ApplyAttackStats();
    }

    private void ApplyAttackStats()
    {
        if (_attack == null)
        {
            return;
        }

        _attack.ApplyStats(damageMult, fireRateMult, rangeMult, sizeMult, lifetimeMult, projectileCount, projectilePierceBonus, weaponDamageMult);
        _attack.SetWeaponDamageMultipliers(straightWeaponDamageMult, boomerangWeaponDamageMult, novaWeaponDamageMult);
        _attack.SetWeaponRangeMultipliers(straightWeaponRangeMult, boomerangWeaponRangeMult, novaWeaponRangeMult);
        _attack.SetNovaBonusCount(novaBonusCount);
        _attack.SetWeaponEnabled(AutoAttack.WeaponType.Straight, true);
        _attack.SetWeaponEnabled(AutoAttack.WeaponType.Boomerang, boomerangUnlocked);
        _attack.SetWeaponEnabled(AutoAttack.WeaponType.Nova, novaUnlocked);
    }

    private void ApplyDifficultyScaling()
    {
        if (_spawner == null)
        {
            return;
        }

        float newInterval = Mathf.Max(minSpawnInterval, spawnInterval - ElapsedTime * spawnIntervalDecayPerSec);
        _spawner.SpawnInterval = newInterval;

        int extra = Mathf.FloorToInt((ElapsedTime / 60f) * maxEnemiesPerMinute);
        _spawner.MaxEnemies = maxEnemies + extra;
    }

    private void DisableNetworkUI()
    {
        var ui = FindObjectOfType<NetworkStartUI>();
        if (ui != null)
        {
            ui.gameObject.SetActive(false);
        }

        var canvas = GameObject.Find("NetworkStartCanvas");
        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    private void EnsureCameraFollow()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        if (cam.GetComponent<CameraFollow>() == null)
        {
            cam.gameObject.AddComponent<CameraFollow>();
        }
    }

    private void EnsureMinimap()
    {
        if (GetComponent<Minimap>() == null)
        {
            gameObject.AddComponent<Minimap>();
        }
    }

    private void EnsureMapBorder()
    {
        var border = GetComponent<MapBorder>();
        if (border == null)
        {
            border = gameObject.AddComponent<MapBorder>();
        }

        border.SetBounds(mapHalfSize);
    }

    public Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, -mapHalfSize.x, mapHalfSize.x),
            Mathf.Clamp(position.y, -mapHalfSize.y, mapHalfSize.y),
            position.z);
    }

    private void LevelUpStraightWeapon()
    {
        straightWeaponLevel += 1;
        straightWeaponDamageMult *= 1.20f;

        if (straightWeaponLevel % 3 == 0)
        {
            projectileCount += 1;
        }

        if (straightWeaponLevel % 4 == 0)
        {
            projectilePierceBonus += 1;
        }
    }

    private void LevelUpBoomerangWeapon()
    {
        if (!boomerangUnlocked)
        {
            UnlockBoomerang();
        }

        boomerangWeaponLevel += 1;
        boomerangWeaponDamageMult *= 1.20f;

        if (boomerangWeaponLevel % 4 == 0)
        {
            projectilePierceBonus += 1;
        }
    }

    private void LevelUpNovaWeapon()
    {
        if (!novaUnlocked)
        {
            UnlockNova();
        }

        novaWeaponLevel += 1;
        novaWeaponDamageMult *= 1.20f;

        if (novaWeaponLevel % 3 == 0)
        {
            novaBonusCount += 2;
        }
    }

    private void UnlockBoomerang()
    {
        boomerangUnlocked = true;
        if (boomerangWeaponLevel < 1)
        {
            boomerangWeaponLevel = 1;
        }
    }

    private void UnlockNova()
    {
        novaUnlocked = true;
        if (novaWeaponLevel < 1)
        {
            novaWeaponLevel = 1;
        }
    }

    private string BuildWeaponAcquireText(string name, int currentLevel)
    {
        int nextLevel = Mathf.Max(1, currentLevel + 1);
        return $"{name}\n레벨 {currentLevel} -> {nextLevel}\n피해량 1 -> 1\n속도 1 -> 1\n투사체 1 -> 1\n관통 0 -> 0";
    }

    private string BuildStraightUpgradeText()
    {
        int nextLevel = straightWeaponLevel + 1;
        float nextDamage = straightWeaponDamageMult * 1.20f;
        int nextProjectile = projectileCount + (nextLevel % 3 == 0 ? 1 : 0);
        int nextPierce = projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        return BuildWeaponUpgradeText("기본 무기", straightWeaponLevel, nextLevel, straightWeaponDamageMult, nextDamage, 1f, 1f, projectileCount, nextProjectile, projectilePierceBonus, nextPierce);
    }

    private string BuildBoomerangUpgradeText()
    {
        int nextLevel = boomerangWeaponLevel + 1;
        float nextDamage = boomerangWeaponDamageMult * 1.20f;
        int nextPierce = projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        return BuildWeaponUpgradeText("부메랑", boomerangWeaponLevel, nextLevel, boomerangWeaponDamageMult, nextDamage, 1f, 1f, projectileCount, projectileCount, projectilePierceBonus, nextPierce);
    }

    private string BuildNovaUpgradeText()
    {
        int nextLevel = novaWeaponLevel + 1;
        float nextDamage = novaWeaponDamageMult * 1.20f;
        int currentCount = 8 + novaBonusCount;
        int nextCount = currentCount + (nextLevel % 3 == 0 ? 2 : 0);
        return BuildWeaponUpgradeText("노바", novaWeaponLevel, nextLevel, novaWeaponDamageMult, nextDamage, 1f, 1f, currentCount, nextCount, projectilePierceBonus, projectilePierceBonus);
    }

    private string BuildWeaponUpgradeText(string name, int currentLevel, int nextLevel, float currentDamage, float nextDamage, float currentRate, float nextRate, int currentProjectile, int nextProjectile, int currentPierce, int nextPierce)
    {
        return $"{name}\n레벨 {currentLevel} -> {nextLevel}\n피해량 {currentDamage:0.##} -> {nextDamage:0.##}\n속도 {currentRate:0.##} -> {nextRate:0.##}\n투사체 {currentProjectile} -> {nextProjectile}\n관통 {currentPierce} -> {nextPierce}";
    }

    private string BuildPercentStatText(string label, float currentMult, float nextMult)
    {
        return $"{label} {currentMult * 100f:0.#}% -> {nextMult * 100f:0.#}%";
    }

    private string BuildValueStatText(string label, float currentValue, float nextValue)
    {
        return $"{label} {currentValue:0.#} -> {nextValue:0.#}";
    }

    private void OnGUI()
    {
        if (!_choosingUpgrade)
        {
            return;
        }

        const int columns = 3;
        const float boxHeight = 200f;
        const float gap = 12f;
        const float topPadding = 36f;
        const float sidePadding = 12f;

        int count = _options.Count;
        int rows = Mathf.CeilToInt(count / (float)columns);

        float maxWidth = Screen.width - 40f;
        float boxWidth = Mathf.Floor((maxWidth - sidePadding * 2f - (columns - 1) * gap) / columns);
        float w = columns * boxWidth + (columns - 1) * gap + sidePadding * 2f;
        float h = topPadding + rows * boxHeight + (rows - 1) * gap + sidePadding;
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        GUI.Box(new Rect(x, y, w, h), "레벨업 선택");

        var style = new GUIStyle(GUI.skin.button);
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        style.fontSize = 13;

        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            float bx = x + sidePadding + col * (boxWidth + gap);
            float by = y + topPadding + row * (boxHeight + gap);

            var opt = _options[i];
            if (GUI.Button(new Rect(bx, by, boxWidth, boxHeight), $"{i + 1}. {opt.Title}\n{opt.Desc}", style))
            {
                ApplyUpgrade(i);
            }
        }
    }

    private class UpgradeOption
    {
        public string Title;
        public System.Func<string> DescProvider;
        public System.Action Apply;

        public string Desc => DescProvider != null ? DescProvider() : string.Empty;

        public UpgradeOption(string title, System.Func<string> descProvider, System.Action apply)
        {
            Title = title;
            DescProvider = descProvider;
            Apply = apply;
        }
    }
}
