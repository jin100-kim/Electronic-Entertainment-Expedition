using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
    [SerializeField]
    private float amount = 1f;

    [SerializeField]
    private float magnetScanInterval = 0.2f;

    private Experience _magnetTarget;
    private float _nextScanTime;

    public void SetAmount(float value)
    {
        amount = Mathf.Max(0.01f, value);
    }

    private void Update()
    {
        if (Time.unscaledTime >= _nextScanTime || _magnetTarget == null)
        {
            _magnetTarget = FindClosestTarget();
            _nextScanTime = Time.unscaledTime + magnetScanInterval;
        }

        if (_magnetTarget == null)
        {
            return;
        }

        float range = _magnetTarget.MagnetRange;
        if (range <= 0f)
        {
            return;
        }

        Vector3 toTarget = _magnetTarget.transform.position - transform.position;
        float dist = toTarget.magnitude;
        if (dist > range)
        {
            return;
        }

        float speed = _magnetTarget.MagnetSpeed;
        if (speed <= 0f)
        {
            return;
        }

        Vector3 step = toTarget.normalized * speed * Time.unscaledDeltaTime;
        if (step.sqrMagnitude >= toTarget.sqrMagnitude)
        {
            transform.position = _magnetTarget.transform.position;
        }
        else
        {
            transform.position += step;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var xp = other.GetComponent<Experience>();
        if (xp == null)
        {
            return;
        }

        xp.AddXp(amount);
        Destroy(gameObject);
    }

    private Experience FindClosestTarget()
    {
        Experience closest = null;
        float bestSqr = float.MaxValue;
        var targets = FindObjectsOfType<Experience>();
        for (int i = 0; i < targets.Length; i++)
        {
            var target = targets[i];
            if (target == null)
            {
                continue;
            }

            Vector3 delta = target.transform.position - transform.position;
            float sqr = delta.sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = target;
            }
        }

        return closest;
    }
}
