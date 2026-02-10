using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField]
    private Color flashColor = new Color(1f, 0.55f, 0.55f, 1f);

    [SerializeField]
    private float flashDuration = 0.08f;

    [SerializeField]
    private bool onlyIfNoHurtAnimation = true;

    private SpriteRenderer[] _renderers;
    private Color[] _baseColors;
    private Health _health;
    private float _lastHealth;
    private float _timer;
    private bool _canFlash = true;

    private void Awake()
    {
        _health = GetComponent<Health>();
        EnsureRenderers();
        _lastHealth = _health != null ? _health.CurrentHealth : 0f;

        if (onlyIfNoHurtAnimation && HasHurtAnimation())
        {
            _canFlash = false;
            enabled = false;
        }
    }

    private void Update()
    {
        if (!_canFlash || _health == null)
        {
            return;
        }

        EnsureRenderers();
        if (_renderers == null || _renderers.Length == 0)
        {
            return;
        }

        float current = _health.CurrentHealth;
        if (current < _lastHealth - 0.001f)
        {
            TriggerFlash();
        }
        _lastHealth = current;

        if (_timer > 0f)
        {
            _timer -= Time.deltaTime;
            float t = Mathf.Clamp01(_timer / Mathf.Max(0.01f, flashDuration));
            ApplyFlash(t);
            if (_timer <= 0f)
            {
                RestoreColors();
            }
        }
    }

    private void TriggerFlash()
    {
        CaptureBaseColors();
        _timer = flashDuration;
        ApplyFlash(1f);
    }

    private void CaptureBaseColors()
    {
        if (_baseColors == null)
        {
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            _baseColors[i] = renderer != null ? renderer.color : Color.white;
        }
    }

    private void EnsureRenderers()
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (_renderers == null || _renderers.Length == 0)
        {
            _baseColors = null;
            return;
        }

        if (_baseColors == null || _baseColors.Length != _renderers.Length)
        {
            _baseColors = new Color[_renderers.Length];
        }
    }

    private void ApplyFlash(float t)
    {
        if (_baseColors == null)
        {
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.color = Color.Lerp(_baseColors[i], flashColor, t);
        }
    }

    private void RestoreColors()
    {
        if (_baseColors == null)
        {
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            if (renderer != null)
            {
                renderer.color = _baseColors[i];
            }
        }
    }

    private bool HasHurtAnimation()
    {
        var animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return false;
        }

        int hurtHash = Animator.StringToHash("Hurt");
        bool hasState = animator.HasState(0, hurtHash);
        bool hasTrigger = false;
        var parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger && parameters[i].name == "Hurt")
            {
                hasTrigger = true;
                break;
            }
        }

        return hasState && hasTrigger;
    }

    private void OnDisable()
    {
        RestoreColors();
    }
}
