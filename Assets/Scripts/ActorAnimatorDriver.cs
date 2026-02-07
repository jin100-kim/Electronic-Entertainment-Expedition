using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ActorAnimatorDriver : MonoBehaviour
{
    [SerializeField]
    private string moveParam = "IsMoving";

    [SerializeField]
    private string deadParam = "IsDead";

    [SerializeField]
    private string hurtTrigger = "Hurt";

    [SerializeField]
    private float moveThreshold = 0.02f;

    private Animator _animator;
    private Health _health;
    private Vector3 _lastPosition;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
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

    private void Update()
    {
        if (_animator == null)
        {
            return;
        }

        Vector3 delta = transform.position - _lastPosition;
        _lastPosition = transform.position;

        if (!string.IsNullOrEmpty(moveParam))
        {
            bool isMoving = delta.sqrMagnitude > moveThreshold * moveThreshold;
            _animator.SetBool(moveParam, isMoving);
        }
    }

    private void OnDamaged(float amount)
    {
        if (_animator == null || string.IsNullOrEmpty(hurtTrigger))
        {
            return;
        }

        _animator.SetTrigger(hurtTrigger);
    }

    private void OnDied()
    {
        if (_animator == null || string.IsNullOrEmpty(deadParam))
        {
            return;
        }

        _animator.SetBool(deadParam, true);
    }
}
