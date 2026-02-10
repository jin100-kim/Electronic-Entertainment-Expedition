using UnityEngine;

public static class ElementSystem
{
    private static GameConfig _cachedGameConfig;
    private static ElementSystemConfig _cachedConfig;

    private static ElementSystemConfig Config
    {
        get
        {
            var config = GameConfig.LoadOrCreate();
            if (config == null)
            {
                return new ElementSystemConfig();
            }

            if (_cachedGameConfig != config || _cachedConfig == null)
            {
                _cachedGameConfig = config;
                _cachedConfig = config.elementSystem ?? new ElementSystemConfig();
            }

            return _cachedConfig;
        }
    }

    public static void ApplyOnHit(ElementType element, StatusEffectType statusEffect, float powerMult, float durationMult, ElementStatus targetStatus)
    {
        if (targetStatus == null)
        {
            return;
        }

        var settings = Config;
        if (settings == null || !settings.enabled)
        {
            return;
        }

        if (element == ElementType.None && statusEffect == StatusEffectType.None)
        {
            return;
        }

        if (element != ElementType.None)
        {
            var elementCfg = GetElementConfig(settings, element);
            ApplyElementTag(settings, targetStatus, element, durationMult);

            if (statusEffect == StatusEffectType.None && elementCfg != null)
            {
                statusEffect = elementCfg.defaultStatusEffect;
            }
        }

        if (statusEffect != StatusEffectType.None)
        {
            var statusCfg = GetStatusConfig(settings, statusEffect);
            float duration = statusCfg != null ? statusCfg.baseDuration : settings.defaultStatusDuration;
            float power = statusCfg != null ? statusCfg.basePower : 1f;
            duration *= Mathf.Max(0.1f, durationMult);
            power *= Mathf.Max(0.1f, powerMult);
            targetStatus.ApplyStatus(statusEffect, duration, power);
        }
    }

    public static void ApplyElementsOnHit(ElementType first, ElementType second, ElementType third, int count, ElementStatus targetStatus)
    {
        if (targetStatus == null || count <= 0)
        {
            return;
        }

        var settings = Config;
        if (settings == null || !settings.enabled)
        {
            return;
        }

        if (count >= 1)
        {
            ApplyElementTag(settings, targetStatus, first, 1f);
        }
        if (count >= 2)
        {
            ApplyElementTag(settings, targetStatus, second, 1f);
        }
        if (count >= 3)
        {
            ApplyElementTag(settings, targetStatus, third, 1f);
        }
    }

    public static ElementConfig GetElementConfig(ElementType element)
    {
        return GetElementConfig(Config, element);
    }

    public static StatusEffectConfig GetStatusConfig(StatusEffectType effect)
    {
        return GetStatusConfig(Config, effect);
    }

    private static ElementConfig GetElementConfig(ElementSystemConfig settings, ElementType element)
    {
        if (settings == null || settings.elements == null)
        {
            return null;
        }

        var list = settings.elements;
        for (int i = 0; i < list.Length; i++)
        {
            var cfg = list[i];
            if (cfg != null && cfg.element == element)
            {
                return cfg;
            }
        }

        return null;
    }

    private static void ApplyElementTag(ElementSystemConfig settings, ElementStatus targetStatus, ElementType element, float durationMult)
    {
        if (element == ElementType.None || targetStatus == null)
        {
            return;
        }

        var elementCfg = GetElementConfig(settings, element);
        float duration = elementCfg != null ? elementCfg.defaultDuration : settings.defaultElementDuration;
        duration *= Mathf.Max(0.1f, durationMult);
        targetStatus.ApplyElement(element, duration);
    }

    private static StatusEffectConfig GetStatusConfig(ElementSystemConfig settings, StatusEffectType effect)
    {
        if (settings == null || settings.statusEffects == null)
        {
            return null;
        }

        var list = settings.statusEffects;
        for (int i = 0; i < list.Length; i++)
        {
            var cfg = list[i];
            if (cfg != null && cfg.effect == effect)
            {
                return cfg;
            }
        }

        return null;
    }
}
