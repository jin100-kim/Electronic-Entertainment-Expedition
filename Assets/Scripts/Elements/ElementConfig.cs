using UnityEngine;

[System.Serializable]
public class ElementSystemConfig
{
    public bool enabled = true;
    public int maxElementsPerWeapon = 3;
    public float defaultElementDuration = 2f;
    public float defaultStatusDuration = 2f;

    public ElementConfig[] elements = new[]
    {
        new ElementConfig { element = ElementType.Fire, displayName = "Fire", uiColor = new Color(1f, 0.4f, 0.2f, 1f), defaultDuration = 2f, defaultStatusEffect = StatusEffectType.Fire },
        new ElementConfig { element = ElementType.Water, displayName = "Water", uiColor = new Color(0.2f, 0.6f, 1f, 1f), defaultDuration = 2f, defaultStatusEffect = StatusEffectType.Water },
        new ElementConfig { element = ElementType.Wind, displayName = "Wind", uiColor = new Color(0.7f, 1f, 0.7f, 1f), defaultDuration = 2f, defaultStatusEffect = StatusEffectType.Wind },
        new ElementConfig { element = ElementType.Earth, displayName = "Earth", uiColor = new Color(0.6f, 0.45f, 0.25f, 1f), defaultDuration = 2f, defaultStatusEffect = StatusEffectType.Earth }
    };

    public StatusEffectConfig[] statusEffects = new[]
    {
        new StatusEffectConfig { effect = StatusEffectType.Fire, baseDuration = 2f, basePower = 1f },
        new StatusEffectConfig { effect = StatusEffectType.Water, baseDuration = 2f, basePower = 1f },
        new StatusEffectConfig { effect = StatusEffectType.Wind, baseDuration = 2f, basePower = 1f },
        new StatusEffectConfig { effect = StatusEffectType.Earth, baseDuration = 2f, basePower = 1f }
    };

    public ElementInteractionRule[] interactions = new ElementInteractionRule[0];
    public ElementComboRule[] combos = new ElementComboRule[0];
}

[System.Serializable]
public class ElementConfig
{
    public ElementType element = ElementType.None;
    public string displayName = "Element";
    public Color uiColor = Color.white;
    public float defaultDuration = 2f;
    public StatusEffectType defaultStatusEffect = StatusEffectType.None;
}

[System.Serializable]
public class StatusEffectConfig
{
    public StatusEffectType effect = StatusEffectType.None;
    public float baseDuration = 2f;
    public float basePower = 1f;
}

[System.Serializable]
public class ElementInteractionRule
{
    public ElementType source = ElementType.None;
    public ElementType target = ElementType.None;
    public ElementInteractionResult result = ElementInteractionResult.None;
    public StatusEffectType applyStatus = StatusEffectType.None;
    public float damageMultiplier = 1f;
    public float durationMultiplier = 1f;
}

[System.Serializable]
public class ElementComboRule
{
    public ElementType first = ElementType.None;
    public ElementType second = ElementType.None;
    public ElementType third = ElementType.None;
    [Range(2, 3)]
    public int requiredCount = 2;
    public ElementInteractionResult result = ElementInteractionResult.None;
    public StatusEffectType applyStatus = StatusEffectType.None;
    public float damageMultiplier = 1f;
    public float durationMultiplier = 1f;
}
