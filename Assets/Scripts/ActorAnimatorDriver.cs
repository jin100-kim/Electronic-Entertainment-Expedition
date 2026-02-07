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

    private Animator _animator;
    private Health _health;
    private Vector3 _lastPosition;
    private bool _isMoving;
    private float _smoothedSpeed;

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
        if (_animator == null || _animator.runtimeAnimatorController == null || string.IsNullOrEmpty(hurtTrigger))
        {
            return;
        }

        _animator.SetTrigger(hurtTrigger);
    }

    private void OnDied()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null || string.IsNullOrEmpty(deadParam))
        {
            return;
        }

        _animator.SetBool(deadParam, true);
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
