using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkTransform))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private Color playerColor = new Color(0.2f, 0.9f, 0.3f, 1f);

    [SerializeField]
    private Color[] playerPalette = new[]
    {
        new Color(0.2f, 0.9f, 0.3f, 1f),
        new Color(1f, 0.85f, 0.2f, 1f),
        new Color(0.3f, 0.7f, 1f, 1f),
        new Color(1f, 0.4f, 0.6f, 1f)
    };

    [SerializeField]
    private Vector3 shadowOffset = new Vector3(0f, -0.25f, 0f);

    [SerializeField]
    private Vector3 shadowScale = new Vector3(0.6f, 0.25f, 1f);

    [SerializeField]
    private float shadowAlpha = 0.6f;

    [SerializeField]
    private bool allowOfflineControl = true;

    [SerializeField]
    private bool autoPlayEnabled = false;

    [SerializeField]
    private float visualScale = 4f;

    [Header("Auto Play")]
    [SerializeField]
    private bool autoSeekXp = true;

    [SerializeField]
    private bool autoXpPriority = false;

    [SerializeField]
    private float autoXpSeekRange = 5f;

    [SerializeField]
    private float autoMinDistance = 2.5f;

    [SerializeField]
    private float autoMaxDistance = 4.0f;

    [SerializeField]
    private float autoOrbitStrength = 0.8f;

    [SerializeField]
    private float autoKeepDistanceStrength = 0.6f;

    [SerializeField]
    private float autoCenterPull = 0.9f;

    [SerializeField]
    private float autoSmooth = 10f;

    [SerializeField]
    private float autoMidCenterPull = 0.1f;

    private const int SpriteSize = 50;
    private float _moveSpeedMult = 1f;
    private Vector2 _autoInputCurrent;
    private Transform _visualRoot;
    private SpriteRenderer _visualRenderer;
    private SpriteRenderer _shadowRenderer;
    private Vector3 _lastPosition;

    private void Awake()
    {
        _lastPosition = transform.position;
        EnsureVisual();
        EnsureShadow();
        EnsurePhysics();
        EnsureHealth();
        EnsureStatusBars();
        EnsureDamageVignette();
        EnsureVisuals();
    }

    private void Start()
    {
        ApplyPlayerColor();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ApplyPlayerColor();
    }

    private void Update()
    {
        if (!CanReadInput())
        {
            return;
        }

        Vector2 input = autoPlayEnabled ? GetAutoInput() : ReadMovement();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        float speed = moveSpeed * _moveSpeedMult;
        Vector2 delta = input * speed * Time.deltaTime;
        transform.Translate(delta, Space.World);
        UpdateFacingFromInput(input);

        if (GameSession.Instance != null)
        {
            transform.position = GameSession.Instance.ClampToBounds(transform.position);
        }
    }

    public void SetMoveSpeedMultiplier(float value)
    {
        _moveSpeedMult = Mathf.Max(0.1f, value);
    }

    public void SetAutoPlay(bool enabled)
    {
        autoPlayEnabled = enabled;
    }

    public bool IsAutoPlayEnabled => autoPlayEnabled;

    private bool CanReadInput()
    {
        if (IsOwner)
        {
            return true;
        }

        if (!allowOfflineControl)
        {
            return false;
        }

        return NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening;
    }

    private static Vector2 ReadMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float x = 0f;
        float y = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

        return new Vector2(x, y);
    }

    private Vector2 GetAutoInput()
    {
        EnemyController closest = null;
        float bestSqr = float.MaxValue;
        var enemies = FindObjectsOfType<EnemyController>();
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            float sqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = enemy;
            }
        }

        ExperiencePickup closestXp = null;
        float bestXpSqr = float.MaxValue;
        if (autoSeekXp)
        {
            float rangeSqr = autoXpSeekRange * autoXpSeekRange;
            var pickups = FindObjectsOfType<ExperiencePickup>();
            for (int i = 0; i < pickups.Length; i++)
            {
                var pickup = pickups[i];
                if (pickup == null)
                {
                    continue;
                }

                float sqr = (pickup.transform.position - transform.position).sqrMagnitude;
                if (sqr <= rangeSqr && sqr < bestXpSqr)
                {
                    bestXpSqr = sqr;
                    closestXp = pickup;
                }
            }
        }

        bool hasEnemy = false;
        bool enemyInBand = false;
        Vector2 toEnemy = Vector2.zero;
        float enemyDist = 0f;
        if (closest != null)
        {
            toEnemy = (closest.transform.position - transform.position);
            enemyDist = toEnemy.magnitude;
            if (enemyDist > 0.001f)
            {
                hasEnemy = true;
                enemyInBand = enemyDist >= autoMinDistance && enemyDist <= autoMaxDistance;
            }
        }

        Vector2 desired = Vector2.zero;
        bool hasTarget = false;
        if (closestXp != null && (autoXpPriority || !hasEnemy || enemyInBand))
        {
            hasTarget = true;
            Vector2 toXp = (closestXp.transform.position - transform.position);
            float dist = toXp.magnitude;
            if (dist > 0.001f)
            {
                desired = toXp / dist;
            }
        }
        else if (hasEnemy)
        {
            hasTarget = true;
            if (enemyDist > 0.001f)
            {
                Vector2 dir = toEnemy / enemyDist;
                if (enemyDist < autoMinDistance)
                {
                    desired = -dir;
                }
                else if (enemyDist > autoMaxDistance)
                {
                    desired = dir;
                }
                else
                {
                    desired = Vector2.zero;
                }
            }
        }

        if (GameSession.Instance != null)
        {
            if (hasTarget && desired == Vector2.zero && autoMidCenterPull > 0f)
            {
                Vector2 toCenter = -((Vector2)transform.position);
                if (toCenter.sqrMagnitude > 0.0001f)
                {
                    desired += toCenter.normalized * autoMidCenterPull;
                }
            }

            Vector2 bounds = GameSession.Instance.MapHalfSize;
            Vector2 pos = transform.position;
            float margin = 1.2f;
            Vector2 centerPush = Vector2.zero;
            if (Mathf.Abs(pos.x) > bounds.x - margin)
            {
                centerPush.x = -Mathf.Sign(pos.x);
            }
            if (Mathf.Abs(pos.y) > bounds.y - margin)
            {
                centerPush.y = -Mathf.Sign(pos.y);
            }
            if (centerPush != Vector2.zero)
            {
                desired += centerPush * autoCenterPull;
            }
        }

        if (desired == Vector2.zero && !hasTarget)
        {
            desired = new Vector2(Mathf.Sin(Time.time * 0.7f), Mathf.Cos(Time.time * 0.7f));
        }

        if (desired.sqrMagnitude > 1f)
        {
            desired.Normalize();
        }

        float lerp = 1f - Mathf.Exp(-autoSmooth * Time.deltaTime);
        _autoInputCurrent = Vector2.Lerp(_autoInputCurrent, desired, lerp);
        return _autoInputCurrent;
    }

    private void EnsureVisual()
    {
        var rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        _visualRoot = GetOrCreateVisualRoot();
        _visualRoot.localScale = Vector3.one * visualScale;

        _visualRenderer = _visualRoot.GetComponent<SpriteRenderer>();
        if (_visualRenderer == null)
        {
            _visualRenderer = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
        }

        var animator = _visualRoot.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            _visualRenderer.color = Color.white;
            return;
        }

        if (_visualRenderer.sprite == null)
        {
            _visualRenderer.sprite = CreateCircleSprite(SpriteSize);
        }

        _visualRenderer.color = Color.white;
    }

    private void EnsureShadow()
    {
        var existing = transform.Find("Shadow");
        if (existing == null)
        {
            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(transform, false);
            shadow.transform.localPosition = shadowOffset;
            shadow.transform.localScale = shadowScale;
            _shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        }
        else
        {
            _shadowRenderer = existing.GetComponent<SpriteRenderer>();
            if (_shadowRenderer == null)
            {
                _shadowRenderer = existing.gameObject.AddComponent<SpriteRenderer>();
            }
            existing.localPosition = shadowOffset;
            existing.localScale = shadowScale;
        }

        if (_shadowRenderer.sprite == null)
        {
            _shadowRenderer.sprite = CreateCircleSprite(SpriteSize);
        }

        UpdateShadowColor();
        _shadowRenderer.sortingOrder = -1;
    }

    private void LateUpdate()
    {
        Vector3 current = transform.position;
        Vector3 delta = current - _lastPosition;
        if (Mathf.Abs(delta.x) > 0.001f)
        {
            SetFacing(delta.x < 0f);
        }
        _lastPosition = current;
    }

    private void UpdateFacingFromInput(Vector2 input)
    {
        if (Mathf.Abs(input.x) < 0.001f)
        {
            return;
        }

        SetFacing(input.x < 0f);
    }

    private void SetFacing(bool faceLeft)
    {
        if (_visualRenderer == null)
        {
            return;
        }

        _visualRenderer.flipX = faceLeft;
    }

    private void ApplyPlayerColor()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (playerPalette != null && playerPalette.Length > 0)
            {
                int index = (int)(OwnerClientId % (ulong)playerPalette.Length);
                playerColor = playerPalette[index];
            }
        }

        UpdateShadowColor();
    }

    private void UpdateShadowColor()
    {
        if (_shadowRenderer == null)
        {
            return;
        }

        var shadowColor = playerColor;
        shadowColor.a = Mathf.Clamp01(shadowAlpha);
        _shadowRenderer.color = shadowColor;
    }

    private void EnsurePhysics()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }

        col.isTrigger = true;
        col.radius = 0.5f;
    }

    private void EnsureHealth()
    {
        if (GetComponent<Health>() == null)
        {
            gameObject.AddComponent<Health>();
        }
    }

    private void EnsureStatusBars()
    {
        if (GetComponent<PlayerStatusBars>() == null)
        {
            gameObject.AddComponent<PlayerStatusBars>();
        }
    }

    private void EnsureDamageVignette()
    {
        if (GetComponent<PlayerDamageVignette>() == null)
        {
            gameObject.AddComponent<PlayerDamageVignette>();
        }
    }

    private void EnsureVisuals()
    {
        if (GetComponent<PlayerVisuals>() == null)
        {
            gameObject.AddComponent<PlayerVisuals>();
        }
    }

    private Transform GetOrCreateVisualRoot()
    {
        var existing = transform.Find("Visuals");
        if (existing != null)
        {
            EnsureAligner(existing.gameObject);
            return existing;
        }

        var root = new GameObject("Visuals");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;
        EnsureAligner(root);
        return root.transform;
    }

    private static void EnsureAligner(GameObject root)
    {
        if (root.GetComponent<VisualsAligner>() == null)
        {
            root.AddComponent<VisualsAligner>();
        }
    }

    private static Sprite CreateCircleSprite(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var colors = new Color32[size * size];
        float r = (size - 1) * 0.5f;
        float cx = r;
        float cy = r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                bool inside = (dx * dx + dy * dy) <= r * r;
                colors[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
