using System.Collections;
using UnityEngine;

public class ActorAnimatorDriver : MonoBehaviour
{
    [SerializeField]
    private string moveParam = "IsMoving";

    [SerializeField]
    private string deadParam = "IsDead";

    [SerializeField]
    private string hurtTrigger = "Hurt";

    [SerializeField]
    private float moveStartThreshold = 0.12f;

    [SerializeField]
    private float moveStopThreshold = 0.06f;

    [SerializeField]
    private float moveSmoothing = 12f;

    [SerializeField]
    private bool useUnscaledTimeOnDeath = true;

    private Animator _animator;
    private Health _health;
    private EnemyController _enemy;
    private Vector3 _lastPosition;
    private bool _isMoving;
    private float _smoothedSpeed;
    private bool _isDead;
    private Coroutine _deathRoutine;

    private void Awake()
    {
        ResolveAnimator();
        _health = GetComponent<Health>();
        _enemy = GetComponent<EnemyController>();
        _lastPosition = transform.position;

        if (_health != null)
        {
            _health.OnDamaged += OnDamaged;
            _health.OnDied += OnDied;
        }
    }

    private void OnEnable()
    {
        if (_health != null && _health.IsDead)
        {
            OnDied();
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDamaged -= OnDamaged;
            _health.OnDied -= OnDied;
        }
    }

    private void LateUpdate()
    {
        if (_isDead)
        {
            return;
        }

        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            ResolveAnimator();
        }

        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            return;
        }

        if (!_isDead)
        {
            if (!_animator.enabled)
            {
                _animator.enabled = true;
            }

            if (_animator.speed <= 0f)
            {
                _animator.speed = 1f;
            }

            if (_animator.updateMode != AnimatorUpdateMode.Normal)
            {
                _animator.updateMode = AnimatorUpdateMode.Normal;
            }
        }

        Vector3 delta = transform.position - _lastPosition;
        _lastPosition = transform.position;

        if (!string.IsNullOrEmpty(moveParam))
        {
            float speed = Time.deltaTime > 0.0001f ? delta.magnitude / Time.deltaTime : 0f;
            float lerp = 1f - Mathf.Exp(-moveSmoothing * Time.deltaTime);
            _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, speed, lerp);

            if (!_isMoving && _smoothedSpeed >= moveStartThreshold)
            {
                _isMoving = true;
            }
            else if (_isMoving && _smoothedSpeed <= moveStopThreshold)
            {
                _isMoving = false;
            }

            _animator.SetBool(moveParam, _isMoving);
        }

        TryExitHurtState();
    }

    private void OnDamaged(float amount)
    {
        if (_isDead || _animator == null || _animator.runtimeAnimatorController == null || string.IsNullOrEmpty(hurtTrigger))
        {
            return;
        }

        _animator.SetTrigger(hurtTrigger);
    }

    private void OnDied()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        ResolveAnimator();

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
        }
        _deathRoutine = StartCoroutine(PlayDeathAndHold());
    }

    private IEnumerator PlayDeathAndHold()
    {
        float waitElapsed = 0f;
        float waitTimeout = 0.5f;
        while (waitElapsed < waitTimeout && (_animator == null || _animator.runtimeAnimatorController == null))
        {
            ResolveAnimator();
            waitElapsed += useUnscaledTimeOnDeath ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        if (_animator == null || _animator.runtimeAnimatorController == null || string.IsNullOrEmpty(deadParam))
        {
            yield break;
        }

        if (useUnscaledTimeOnDeath)
        {
            _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        _animator.speed = 1f;
        _animator.SetBool(deadParam, true);
        if (!string.IsNullOrEmpty(moveParam))
        {
            _animator.SetBool(moveParam, false);
        }

        int deathHash = Animator.StringToHash("Death");
        if (_animator.HasState(0, deathHash))
        {
            _animator.CrossFade(deathHash, 0.02f, 0, 0f);
        }

        yield return null;

        float waitDuration = 0.15f;
        if (_animator != null)
        {
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Death") && state.length > 0.01f)
            {
                waitDuration = Mathf.Max(waitDuration, state.length);
            }
            else
            {
                float clipDuration = GetDeathClipDuration();
                waitDuration = Mathf.Max(waitDuration, clipDuration);
            }
        }
        if (useUnscaledTimeOnDeath)
        {
            yield return new WaitForSecondsRealtime(waitDuration);
        }
        else
        {
            yield return new WaitForSeconds(waitDuration);
        }

        // Let the death clip finish naturally; EnemyController handles cleanup/fade.
    }

    private void ResolveAnimator()
    {
        _animator = null;

        var visualRoot = transform.Find("Visuals");
        if (visualRoot != null)
        {
            var visualAnimator = visualRoot.GetComponent<Animator>();
            if (visualAnimator != null && visualAnimator.runtimeAnimatorController != null)
            {
                _animator = visualAnimator;
            }
        }

        if (_animator == null)
        {
            var animators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                var anim = animators[i];
                if (anim != null && anim.runtimeAnimatorController != null)
                {
                    _animator = anim;
                    break;
                }
            }
        }

        var rootAnimator = GetComponent<Animator>();
        if (rootAnimator != null && rootAnimator != _animator && rootAnimator.runtimeAnimatorController == null)
        {
            rootAnimator.enabled = false;
        }

        if (_animator != null)
        {
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (!_isDead)
            {
                _animator.speed = 1f;
                _animator.updateMode = AnimatorUpdateMode.Normal;
            }
        }
    }

    private float GetDeathClipDuration()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            return 0f;
        }

        var clips = _animator.runtimeAnimatorController.animationClips;
        if (clips == null)
        {
            return 0f;
        }

        for (int i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            if (clip != null && string.Equals(clip.name, "Death", System.StringComparison.OrdinalIgnoreCase))
            {
                return Mathf.Max(0f, clip.length);
            }
        }

        return 0f;
    }

    private void TryExitHurtState()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null || _enemy == null)
        {
            return;
        }

        if (_enemy.IsStunned)
        {
            return;
        }

        var state = _animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Hurt"))
        {
            return;
        }

        string target = _isMoving ? "Move" : "Idle";
        _animator.CrossFade(target, 0.05f, 0, 0f);
    }
}
