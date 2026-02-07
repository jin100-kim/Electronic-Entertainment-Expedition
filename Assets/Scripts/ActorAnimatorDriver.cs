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
    private Vector3 _lastPosition;
    private bool _isMoving;
    private float _smoothedSpeed;
    private bool _isDead;
    private Coroutine _deathRoutine;

    private void Awake()
    {
        ResolveAnimator();
        _health = GetComponent<Health>();
        _lastPosition = transform.position;

        if (_health != null)
        {
            _health.OnDamaged += OnDamaged;
            _health.OnDied += OnDied;
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

        if (_animator == null || _animator.runtimeAnimatorController == null || string.IsNullOrEmpty(deadParam))
        {
            return;
        }

        _animator.SetBool(deadParam, true);
        if (!string.IsNullOrEmpty(moveParam))
        {
            _animator.SetBool(moveParam, false);
        }

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
        }
        _deathRoutine = StartCoroutine(PlayDeathAndHold());
    }

    private IEnumerator PlayDeathAndHold()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            yield break;
        }

        if (useUnscaledTimeOnDeath)
        {
            _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        _animator.speed = 1f;
        int deathHash = Animator.StringToHash("Death");
        if (!_animator.HasState(0, deathHash))
        {
            yield break;
        }

        _animator.Play(deathHash, 0, 0f);
        yield return null;

        float timeout = 5f;
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            if (_animator == null)
            {
                yield break;
            }

            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Death") && state.normalizedTime >= 1f)
            {
                break;
            }

            elapsed += useUnscaledTimeOnDeath ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        if (_animator != null)
        {
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Death"))
            {
                _animator.speed = 0f;
                _animator.Update(0f);
            }
        }
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
    }
}
