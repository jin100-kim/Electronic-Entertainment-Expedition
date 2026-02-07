using UnityEngine;

public class EnemyTier : MonoBehaviour
{
    public enum Tier
    {
        Normal,
        Elite,
        Boss
    }

    [SerializeField]
    private Tier currentTier = Tier.Normal;

    public Tier CurrentTier => currentTier;

    public void SetTier(Tier tier)
    {
        currentTier = tier;
    }
}
