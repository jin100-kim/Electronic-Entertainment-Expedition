using System.Collections.Generic;
using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    private struct EffectState
    {
        public float remaining;
        public float power;
    }

    private readonly Dictionary<ElementType, float> _elements = new Dictionary<ElementType, float>();
    private readonly Dictionary<StatusEffectType, EffectState> _statusEffects = new Dictionary<StatusEffectType, EffectState>();
    private readonly List<ElementType> _elementCleanup = new List<ElementType>();
    private readonly List<StatusEffectType> _statusCleanup = new List<StatusEffectType>();
    private bool _hasActive;

    public void ApplyElement(ElementType element, float duration)
    {
        if (element == ElementType.None)
        {
            return;
        }

        float time = Mathf.Max(0.01f, duration);
        if (_elements.TryGetValue(element, out var existing))
        {
            _elements[element] = Mathf.Max(existing, time);
        }
        else
        {
            _elements[element] = time;
        }

        _hasActive = true;
    }

    public void ApplyStatus(StatusEffectType effect, float duration, float power)
    {
        if (effect == StatusEffectType.None)
        {
            return;
        }

        float time = Mathf.Max(0.01f, duration);
        float pow = Mathf.Max(0.01f, power);
        if (_statusEffects.TryGetValue(effect, out var existing))
        {
            existing.remaining = Mathf.Max(existing.remaining, time);
            existing.power = Mathf.Max(existing.power, pow);
            _statusEffects[effect] = existing;
        }
        else
        {
            _statusEffects[effect] = new EffectState { remaining = time, power = pow };
        }

        _hasActive = true;
    }

    public bool HasElement(ElementType element)
    {
        return _elements.ContainsKey(element);
    }

    public bool HasStatus(StatusEffectType effect)
    {
        return _statusEffects.ContainsKey(effect);
    }

    private void Update()
    {
        if (!_hasActive)
        {
            return;
        }

        if (_elements.Count > 0)
        {
            _elementCleanup.Clear();
            foreach (var kvp in _elements)
            {
                _elementCleanup.Add(kvp.Key);
            }

            for (int i = 0; i < _elementCleanup.Count; i++)
            {
                var key = _elementCleanup[i];
                float next = _elements[key] - Time.deltaTime;
                if (next <= 0f)
                {
                    _elements.Remove(key);
                }
                else
                {
                    _elements[key] = next;
                }
            }
        }

        if (_statusEffects.Count > 0)
        {
            _statusCleanup.Clear();
            foreach (var kvp in _statusEffects)
            {
                _statusCleanup.Add(kvp.Key);
            }

            for (int i = 0; i < _statusCleanup.Count; i++)
            {
                var key = _statusCleanup[i];
                var state = _statusEffects[key];
                state.remaining -= Time.deltaTime;
                if (state.remaining <= 0f)
                {
                    _statusEffects.Remove(key);
                }
                else
                {
                    _statusEffects[key] = state;
                }
            }
        }

        _hasActive = _elements.Count > 0 || _statusEffects.Count > 0;
    }
}
