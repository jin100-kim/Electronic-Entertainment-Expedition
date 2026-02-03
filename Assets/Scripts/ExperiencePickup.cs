using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
    [SerializeField]
    private float amount = 1f;

    public void SetAmount(float value)
    {
        amount = Mathf.Max(0.01f, value);
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
}
