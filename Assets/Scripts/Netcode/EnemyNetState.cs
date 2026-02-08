using Unity.Netcode;
using UnityEngine;

public class EnemyNetState : NetworkBehaviour
{
    private readonly NetworkVariable<int> _tier = new NetworkVariable<int>((int)EnemyTier.Tier.Normal);
    private readonly NetworkVariable<float> _visualScale = new NetworkVariable<float>(4f);

    public override void OnNetworkSpawn()
    {
        ApplyState();
        _tier.OnValueChanged += OnTierChanged;
        _visualScale.OnValueChanged += OnScaleChanged;
    }

    public override void OnNetworkDespawn()
    {
        _tier.OnValueChanged -= OnTierChanged;
        _visualScale.OnValueChanged -= OnScaleChanged;
        base.OnNetworkDespawn();
    }

    public void SetTier(EnemyTier.Tier tier)
    {
        if (!IsServer)
        {
            return;
        }

        _tier.Value = (int)tier;
        ApplyState();
    }

    public void SetVisualScale(float scale)
    {
        if (!IsServer)
        {
            return;
        }

        _visualScale.Value = Mathf.Max(0.1f, scale);
        ApplyState();
    }

    private void OnTierChanged(int previous, int next)
    {
        ApplyState();
    }

    private void OnScaleChanged(float previous, float next)
    {
        ApplyState();
    }

    private void ApplyState()
    {
        var tier = (EnemyTier.Tier)_tier.Value;
        var tierComp = GetComponent<EnemyTier>();
        if (tierComp != null)
        {
            tierComp.SetTier(tier);
        }

        var visuals = GetComponent<EnemyVisuals>();
        if (visuals != null)
        {
            visuals.SetType(MapVisualType(tier));
            visuals.SetVisualScale(_visualScale.Value);
        }
    }

    private static EnemyVisuals.EnemyVisualType MapVisualType(EnemyTier.Tier tier)
    {
        switch (tier)
        {
            case EnemyTier.Tier.Boss:
                return EnemyVisuals.EnemyVisualType.Skeleton;
            case EnemyTier.Tier.Elite:
                return EnemyVisuals.EnemyVisualType.Mushroom;
            default:
                return EnemyVisuals.EnemyVisualType.Slime;
        }
    }
}
