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
    private WeaponStatsData gunStats = new WeaponStatsData
    {
        displayName = "총",
        level = 1,
        unlocked = true,
        damageMult = 1f,
        fireRateMult = 1.2f,
        rangeMult = 1f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData boomerangStats = new WeaponStatsData
    {
        displayName = "부메랑",
        level = 0,
        unlocked = false,
        damageMult = 1f,
        fireRateMult = 0.8f,
        rangeMult = 0.7f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private WeaponStatsData novaStats = new WeaponStatsData
    {
        displayName = "노바",
        level = 0,
        unlocked = false,
        damageMult = 1f,
        fireRateMult = 0.6f,
        rangeMult = 0.5f,
        bonusProjectiles = 0
    };

    [SerializeField]
    private float moveSpeedMult = 1f;

    [SerializeField]
    private float xpGainMult = 1f;

    [SerializeField]
    private float regenPerSecond = 0f;

    [Header("Start Weapon")]
    [SerializeField]
    private bool requireStartWeaponChoice = true;

    [SerializeField]
    private StartWeapon startWeapon = StartWeapon.Gun;

    public Vector2 MapHalfSize => mapHalfSize;

    private bool StraightUnlocked => gunStats != null && gunStats.unlocked && gunStats.level > 0;

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
    private bool _waitingStartWeaponChoice;
    private bool _autoPlayEnabled;
    private float _autoUpgradeStartTime = -1f;

    private enum StartWeapon
    {
        Gun,
        Boomerang,
        Nova
    }

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

        if (gunStats != null)
        {
            if (requireStartWeaponChoice)
            {
                gunStats.unlocked = false;
                gunStats.level = 0;
            }
            else
            {
                gunStats.unlocked = true;
                gunStats.level = Mathf.Max(1, gunStats.level);
            }
        }

        if (autoStartLocal)
        {
            if (requireStartWeaponChoice)
            {
                _waitingStartWeaponChoice = true;
            }
            else
            {
                StartLocalGame();
            }
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

        if (_choosingUpgrade && _autoPlayEnabled)
        {
            if (_autoUpgradeStartTime < 0f)
            {
                _autoUpgradeStartTime = Time.unscaledTime;
            }

            if (Time.unscaledTime - _autoUpgradeStartTime >= 1f)
            {
                int index = PickAutoUpgradeIndex();
                if (index >= 0)
                {
                    ApplyUpgrade(index);
                }
            }
        }
        else
        {
            _autoUpgradeStartTime = -1f;
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
        bool isNetworked = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        if (!isNetworked && requireStartWeaponChoice && !_waitingStartWeaponChoice)
        {
            _waitingStartWeaponChoice = true;
            _gameStarted = false;
            return;
        }

        _gameStarted = true;

        if (isNetworked)
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
        _gameStarted = true;
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
        _player.SetAutoPlay(_autoPlayEnabled);

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

        _options.Add(new UpgradeOption("공격력 +25%", () => BuildPercentStatText("공격력", damageMult, damageMult + 0.25f), () => damageMult += 0.25f));
        _options.Add(new UpgradeOption("공격속도 +20%", () => BuildPercentStatText("공격속도", fireRateMult, fireRateMult + 0.20f), () => fireRateMult += 0.20f));
        _options.Add(new UpgradeOption("이동속도 +20%", () => BuildPercentStatText("이동속도", moveSpeedMult, moveSpeedMult + 0.20f), () => moveSpeedMult += 0.20f));
        _options.Add(new UpgradeOption("체력 +40", () => BuildValueStatText("최대 체력", PlayerHealth != null ? PlayerHealth.MaxHealth : 0f, (PlayerHealth != null ? PlayerHealth.MaxHealth : 0f) + 40f), () => PlayerHealth?.AddMaxHealth(40f, true)));
        _options.Add(new UpgradeOption("체력재생 +1", () => BuildValueStatText("체력재생", regenPerSecond, regenPerSecond + 1f), () => regenPerSecond += 1f));
        _options.Add(new UpgradeOption("사거리 +25%", () => BuildPercentStatText("사거리", rangeMult, rangeMult + 0.25f), () => rangeMult += 0.25f));
        _options.Add(new UpgradeOption("경험치 +35%", () => BuildPercentStatText("경험치 획득", xpGainMult, xpGainMult + 0.35f), () => xpGainMult += 0.35f));
        if (boomerangStats != null && !boomerangStats.unlocked)
        {
            _options.Add(new UpgradeOption("무기 획득: 부메랑", () => BuildWeaponAcquireText(boomerangStats), () => UnlockBoomerang()));
        }

        if (novaStats != null && !novaStats.unlocked)
        {
            _options.Add(new UpgradeOption("무기 획득: 노바", () => BuildWeaponAcquireText(novaStats), () => UnlockNova()));
        }
        if (gunStats != null && !gunStats.unlocked)
        {
            _options.Add(new UpgradeOption("무기 획득: 총", () => BuildWeaponAcquireText(gunStats), () => UnlockStraight()));
        }
        if (StraightUnlocked)
        {
            _options.Add(new UpgradeOption("총 강화", () => BuildStraightUpgradeText(), () => LevelUpStraightWeapon()));
        }

        if (boomerangStats != null && boomerangStats.unlocked)
        {
            _options.Add(new UpgradeOption("부메랑 강화", () => BuildBoomerangUpgradeText(), () => LevelUpBoomerangWeapon()));
        }

        if (novaStats != null && novaStats.unlocked)
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
        _autoUpgradeStartTime = _autoPlayEnabled ? Time.unscaledTime : -1f;
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
        _autoUpgradeStartTime = -1f;

        // apply updated stats
        _player?.SetMoveSpeedMultiplier(moveSpeedMult);
        PlayerExperience?.SetXpMultiplier(xpGainMult);
        PlayerHealth?.SetRegenPerSecond(regenPerSecond);
        ApplyAttackStats();
    }

    private int PickAutoUpgradeIndex()
    {
        if (_options == null || _options.Count == 0)
        {
            return -1;
        }

        int bestScore = int.MinValue;
        var bestIndices = new List<int>();
        for (int i = 0; i < _options.Count; i++)
        {
            int score = ScoreUpgradeOption(_options[i]);
            if (score > bestScore)
            {
                bestScore = score;
                bestIndices.Clear();
                bestIndices.Add(i);
            }
            else if (score == bestScore)
            {
                bestIndices.Add(i);
            }
        }

        if (bestIndices.Count == 0)
        {
            return -1;
        }

        int pick = Random.Range(0, bestIndices.Count);
        return bestIndices[pick];
    }

    private int ScoreUpgradeOption(UpgradeOption option)
    {
        if (option == null || string.IsNullOrEmpty(option.Title))
        {
            return 0;
        }

        string title = option.Title;
        if (title.Contains("강화") && !title.Contains("무기 획득"))
        {
            return 1;
        }

        if (title.Contains("무기 획득"))
        {
            return 1;
        }

        return 0;
    }

    private void ApplyAttackStats()
    {
        if (_attack == null)
        {
            return;
        }

        _attack.ApplyStats(damageMult, fireRateMult, rangeMult, sizeMult, lifetimeMult, projectileCount, projectilePierceBonus, weaponDamageMult);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Straight, gunStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Boomerang, boomerangStats);
        _attack.SetWeaponStats(AutoAttack.WeaponType.Nova, novaStats);
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
        if (gunStats == null)
        {
            return;
        }

        gunStats.level += 1;
        gunStats.damageMult += 0.20f;
        gunStats.fireRateMult += 0.08f;

        if (gunStats.level % 3 == 0)
        {
            gunStats.bonusProjectiles += 1;
        }

        if (gunStats.level % 4 == 0)
        {
            projectilePierceBonus += 1;
        }
    }

    private void LevelUpBoomerangWeapon()
    {
        if (boomerangStats == null)
        {
            return;
        }

        if (!boomerangStats.unlocked)
        {
            UnlockBoomerang();
        }

        boomerangStats.level += 1;
        boomerangStats.damageMult += 0.20f;
        boomerangStats.fireRateMult += 0.10f;

        if (boomerangStats.level % 4 == 0)
        {
            boomerangStats.bonusProjectiles += 1;
            projectilePierceBonus += 1;
        }
    }

    private void LevelUpNovaWeapon()
    {
        if (novaStats == null)
        {
            return;
        }

        if (!novaStats.unlocked)
        {
            UnlockNova();
        }

        novaStats.level += 1;
        novaStats.damageMult += 0.20f;
        novaStats.fireRateMult += 0.12f;

        if (novaStats.level % 3 == 0)
        {
            novaStats.bonusProjectiles += 2;
        }
    }

    private void UnlockBoomerang()
    {
        if (boomerangStats == null)
        {
            return;
        }

        boomerangStats.unlocked = true;
        if (boomerangStats.level < 1)
        {
            boomerangStats.level = 1;
        }
    }

    private void UnlockNova()
    {
        if (novaStats == null)
        {
            return;
        }

        novaStats.unlocked = true;
        if (novaStats.level < 1)
        {
            novaStats.level = 1;
        }
    }

    private void UnlockStraight()
    {
        if (gunStats == null)
        {
            return;
        }

        gunStats.unlocked = true;
        if (gunStats.level < 1)
        {
            gunStats.level = 1;
        }
    }

    private string BuildWeaponAcquireText(WeaponStatsData stats)
    {
        if (stats == null)
        {
            return string.Empty;
        }

        int currentLevel = stats.level;
        int nextLevel = Mathf.Max(1, currentLevel + 1);
        float dmg = stats.damageMult;
        float rate = stats.fireRateMult;
        return $"{stats.displayName}\n레벨 {currentLevel} -> {nextLevel}\n피해량 {dmg:0.##} -> {dmg:0.##}\n속도 {rate:0.##} -> {rate:0.##}\n투사체 1 -> 1\n관통 0 -> 0";
    }

    private string BuildStraightUpgradeText()
    {
        if (gunStats == null)
        {
            return string.Empty;
        }

        int nextLevel = gunStats.level + 1;
        float nextDamage = gunStats.damageMult + 0.20f;
        int currentProjectile = 1 + gunStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 3 == 0 ? 1 : 0);
        int nextPierce = projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        float nextRate = gunStats.fireRateMult + 0.08f;
        return BuildWeaponUpgradeText(gunStats.displayName, gunStats.level, nextLevel, gunStats.damageMult, nextDamage, gunStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, nextPierce);
    }

    private string BuildBoomerangUpgradeText()
    {
        if (boomerangStats == null)
        {
            return string.Empty;
        }

        int nextLevel = boomerangStats.level + 1;
        float nextDamage = boomerangStats.damageMult + 0.20f;
        int currentProjectile = 1 + boomerangStats.bonusProjectiles;
        int nextProjectile = currentProjectile + (nextLevel % 4 == 0 ? 1 : 0);
        int nextPierce = projectilePierceBonus + (nextLevel % 4 == 0 ? 1 : 0);
        float nextRate = boomerangStats.fireRateMult + 0.10f;
        return BuildWeaponUpgradeText(boomerangStats.displayName, boomerangStats.level, nextLevel, boomerangStats.damageMult, nextDamage, boomerangStats.fireRateMult, nextRate, currentProjectile, nextProjectile, projectilePierceBonus, nextPierce);
    }

    private string BuildNovaUpgradeText()
    {
        if (novaStats == null)
        {
            return string.Empty;
        }

        int nextLevel = novaStats.level + 1;
        float nextDamage = novaStats.damageMult + 0.20f;
        int currentCount = 8 + novaStats.bonusProjectiles;
        int nextCount = currentCount + (nextLevel % 3 == 0 ? 2 : 0);
        float nextRate = novaStats.fireRateMult + 0.12f;
        return BuildWeaponUpgradeText(novaStats.displayName, novaStats.level, nextLevel, novaStats.damageMult, nextDamage, novaStats.fireRateMult, nextRate, currentCount, nextCount, projectilePierceBonus, projectilePierceBonus);
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
        if (_waitingStartWeaponChoice)
        {
            DrawStartWeaponChoice();
        }

        if (_choosingUpgrade)
        {
            DrawUpgradeChoices();
        }

        DrawAutoPlayToggle();
    }

    private void DrawUpgradeChoices()
    {

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

    private void DrawAutoPlayToggle()
    {
        if (_player == null)
        {
            return;
        }

        const float width = 160f;
        const float height = 36f;
        float x = Screen.width - width - 12f;
        float y = Screen.height - height - 12f;

        string label = _autoPlayEnabled ? "AutoPlay: ON" : "AutoPlay: OFF";
        if (GUI.Button(new Rect(x, y, width, height), label))
        {
            _autoPlayEnabled = !_autoPlayEnabled;
            _player.SetAutoPlay(_autoPlayEnabled);
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

    private void DrawStartWeaponChoice()
    {
        const float boxWidth = 560f;
        const float boxHeight = 220f;
        float x = (Screen.width - boxWidth) * 0.5f;
        float y = (Screen.height - boxHeight) * 0.5f;

        GUI.Box(new Rect(x, y, boxWidth, boxHeight), "시작 무기 선택");

        float buttonWidth = 160f;
        float buttonHeight = 120f;
        float gap = 20f;
        float bx = x + (boxWidth - (buttonWidth * 3f + gap * 2f)) * 0.5f;
        float by = y + 70f;

        if (GUI.Button(new Rect(bx, by, buttonWidth, buttonHeight), "총"))
        {
            SelectStartWeapon(StartWeapon.Gun);
        }
        if (GUI.Button(new Rect(bx + buttonWidth + gap, by, buttonWidth, buttonHeight), "부메랑"))
        {
            SelectStartWeapon(StartWeapon.Boomerang);
        }
        if (GUI.Button(new Rect(bx + (buttonWidth + gap) * 2f, by, buttonWidth, buttonHeight), "노바"))
        {
            SelectStartWeapon(StartWeapon.Nova);
        }
    }

    private void SelectStartWeapon(StartWeapon weapon)
    {
        startWeapon = weapon;
        if (gunStats != null)
        {
            gunStats.unlocked = weapon == StartWeapon.Gun;
            gunStats.level = weapon == StartWeapon.Gun ? 1 : 0;
        }

        if (boomerangStats != null)
        {
            boomerangStats.unlocked = weapon == StartWeapon.Boomerang;
            boomerangStats.level = weapon == StartWeapon.Boomerang ? 1 : 0;
        }

        if (novaStats != null)
        {
            novaStats.unlocked = weapon == StartWeapon.Nova;
            novaStats.level = weapon == StartWeapon.Nova ? 1 : 0;
        }

        _waitingStartWeaponChoice = false;
        StartLocalGame();
    }
}
