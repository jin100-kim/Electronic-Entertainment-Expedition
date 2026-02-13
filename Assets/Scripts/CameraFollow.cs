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
            var players = PlayerController.Active;
            PlayerController selected = null;
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p == null)
                {
                    continue;
                }

                if (p.IsOwner)
                {
                    selected = p;
                    break;
                }

                if (selected == null)
                {
                    selected = p;
                }
            }

            if (selected != null)
            {
                target = selected.transform;
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
