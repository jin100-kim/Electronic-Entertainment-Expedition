using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class RestShrineInteractable : MonoBehaviour
{
    [SerializeField]
    private float healRatio = 0.35f;

    [SerializeField]
    private float cooldown = 10f;

    [SerializeField]
    private bool singleUse = false;

    [Header("Visual")]
    [SerializeField]
    private SpriteRenderer indicatorRenderer;

    [SerializeField]
    private Color readyColor = new Color(0.45f, 1f, 0.65f, 1f);

    [SerializeField]
    private Color cooldownColor = new Color(1f, 0.72f, 0.2f, 1f);

    [SerializeField]
    private Color usedColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private float _nextReadyTime;
    private bool _used;

    private void Awake()
    {
        EnsureTriggerCollider();
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        UpdateIndicatorColor();
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    private void Update()
    {
        UpdateIndicatorColor();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryAutoInteract(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryAutoInteract(other);
    }

    private void TryAutoInteract(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        var player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (!CanActivate())
        {
            return;
        }

        Activate(player);
    }

    private bool CanActivate()
    {
        if (_used)
        {
            return false;
        }

        if (NetworkSession.IsActive && !NetworkSession.IsServer)
        {
            return false;
        }

        if (healRatio <= 0f)
        {
            return false;
        }

        return Time.time >= _nextReadyTime;
    }

    private void Activate(PlayerController player)
    {
        var health = player.GetComponent<Health>();
        if (health != null && !health.IsDead)
        {
            float healAmount = health.MaxHealth * Mathf.Clamp01(healRatio);
            health.Heal(healAmount);
        }

        if (singleUse)
        {
            _used = true;
        }
        else
        {
            _nextReadyTime = Time.time + Mathf.Max(0f, cooldown);
        }

        UpdateIndicatorColor();
    }

    private void EnsureTriggerCollider()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void UpdateIndicatorColor()
    {
        if (indicatorRenderer == null)
        {
            return;
        }

        if (_used)
        {
            indicatorRenderer.color = usedColor;
            return;
        }

        bool ready = Time.time >= _nextReadyTime;
        indicatorRenderer.color = ready ? readyColor : cooldownColor;
    }
}
