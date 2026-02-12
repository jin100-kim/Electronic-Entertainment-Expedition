using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float smooth = 12f;

    public void SetTarget(Transform newTarget, bool snap = false)
    {
        target = newTarget;
        if (snap && target != null)
        {
            Vector3 desired = target.position;
            desired.z = transform.position.z;
            transform.position = desired;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position;
        desired.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
    }
}
