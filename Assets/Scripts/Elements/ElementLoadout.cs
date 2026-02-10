using UnityEngine;

public class ElementLoadout : MonoBehaviour
{
    private const int DefaultMaxElements = 3;

    [SerializeField]
    private int maxElementsPerWeapon = DefaultMaxElements;

    private ElementType[] _elements;
    private int[] _counts;
    private bool _initialized;

    private int WeaponCount => System.Enum.GetValues(typeof(AutoAttack.WeaponType)).Length;

    public event System.Action OnLoadoutChanged;

    private void Awake()
    {
        ApplySettings();
        EnsureInitialized();
    }

    public int MaxElementsPerWeapon => maxElementsPerWeapon;

    public void SetMaxElementsPerWeapon(int value)
    {
        maxElementsPerWeapon = Mathf.Clamp(value, 1, DefaultMaxElements);
        EnsureInitialized(true);
        NotifyChanged();
    }

    public void ClearElements(AutoAttack.WeaponType weapon)
    {
        EnsureInitialized();
        int index = (int)weapon;
        _counts[index] = 0;
        int offset = index * maxElementsPerWeapon;
        for (int i = 0; i < maxElementsPerWeapon; i++)
        {
            _elements[offset + i] = ElementType.None;
        }
        NotifyChanged();
    }

    public bool AddElement(AutoAttack.WeaponType weapon, ElementType element)
    {
        if (element == ElementType.None)
        {
            return false;
        }

        EnsureInitialized();
        int index = (int)weapon;
        int count = _counts[index];
        if (count >= maxElementsPerWeapon)
        {
            return false;
        }

        int offset = index * maxElementsPerWeapon;
        for (int i = 0; i < count; i++)
        {
            if (_elements[offset + i] == element)
            {
                return false;
            }
        }

        _elements[offset + count] = element;
        _counts[index] = count + 1;
        NotifyChanged();
        return true;
    }

    public void SetElements(AutoAttack.WeaponType weapon, ElementType first, ElementType second, ElementType third, int count)
    {
        SetElements(weapon, first, second, third, count, true);
    }

    public void SetElements(AutoAttack.WeaponType weapon, ElementType first, ElementType second, ElementType third, int count, bool notify)
    {
        EnsureInitialized();
        int clamped = Mathf.Clamp(count, 0, maxElementsPerWeapon);
        int index = (int)weapon;
        _counts[index] = clamped;
        int offset = index * maxElementsPerWeapon;
        if (maxElementsPerWeapon >= 1)
        {
            _elements[offset + 0] = clamped >= 1 ? first : ElementType.None;
        }
        if (maxElementsPerWeapon >= 2)
        {
            _elements[offset + 1] = clamped >= 2 ? second : ElementType.None;
        }
        if (maxElementsPerWeapon >= 3)
        {
            _elements[offset + 2] = clamped >= 3 ? third : ElementType.None;
        }
        if (notify)
        {
            NotifyChanged();
        }
    }

    public int GetElements(AutoAttack.WeaponType weapon, out ElementType first, out ElementType second, out ElementType third)
    {
        EnsureInitialized();
        int index = (int)weapon;
        int count = _counts[index];
        int offset = index * maxElementsPerWeapon;
        first = count >= 1 && maxElementsPerWeapon >= 1 ? _elements[offset + 0] : ElementType.None;
        second = count >= 2 && maxElementsPerWeapon >= 2 ? _elements[offset + 1] : ElementType.None;
        third = count >= 3 && maxElementsPerWeapon >= 3 ? _elements[offset + 2] : ElementType.None;
        return count;
    }

    private void NotifyChanged()
    {
        OnLoadoutChanged?.Invoke();
    }

    private void ApplySettings()
    {
        var config = GameConfig.LoadOrCreate();
        if (config != null && config.elementSystem != null)
        {
            maxElementsPerWeapon = Mathf.Clamp(config.elementSystem.maxElementsPerWeapon, 1, DefaultMaxElements);
        }
    }

    private void EnsureInitialized(bool force = false)
    {
        if (_initialized && !force)
        {
            return;
        }

        int weaponCount = WeaponCount;
        _counts = new int[weaponCount];
        _elements = new ElementType[weaponCount * maxElementsPerWeapon];
        _initialized = true;
    }
}
