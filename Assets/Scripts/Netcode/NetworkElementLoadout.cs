using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(ElementLoadout))]
public class NetworkElementLoadout : NetworkBehaviour
{
    private const int MaxWeapons = 20;
    private const int ElementsPerWeapon = 3;

    private readonly NetworkVariable<FixedList64Bytes<byte>> _elements = new NetworkVariable<FixedList64Bytes<byte>>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private readonly NetworkVariable<FixedList32Bytes<byte>> _counts = new NetworkVariable<FixedList32Bytes<byte>>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private ElementLoadout _loadout;

    private void Awake()
    {
        _loadout = GetComponent<ElementLoadout>();
    }

    public override void OnNetworkSpawn()
    {
        if (_loadout == null)
        {
            _loadout = GetComponent<ElementLoadout>();
        }

        _elements.OnValueChanged += OnElementsChanged;
        _counts.OnValueChanged += OnCountsChanged;

        if (IsOwner)
        {
            WriteFromLoadout();
            if (_loadout != null)
            {
                _loadout.OnLoadoutChanged += OnLocalChanged;
            }
        }
        else
        {
            ApplyToLoadout();
        }
    }

    public override void OnNetworkDespawn()
    {
        _elements.OnValueChanged -= OnElementsChanged;
        _counts.OnValueChanged -= OnCountsChanged;
        if (_loadout != null)
        {
            _loadout.OnLoadoutChanged -= OnLocalChanged;
        }
        base.OnNetworkDespawn();
    }

    private void OnLocalChanged()
    {
        if (!IsOwner || !IsSpawned)
        {
            return;
        }

        WriteFromLoadout();
    }

    private void OnElementsChanged(FixedList64Bytes<byte> previous, FixedList64Bytes<byte> next)
    {
        if (IsOwner)
        {
            return;
        }

        ApplyToLoadout();
    }

    private void OnCountsChanged(FixedList32Bytes<byte> previous, FixedList32Bytes<byte> next)
    {
        if (IsOwner)
        {
            return;
        }

        ApplyToLoadout();
    }

    private void WriteFromLoadout()
    {
        if (_loadout == null)
        {
            return;
        }

        int weaponCount = Mathf.Min(GetWeaponCount(), MaxWeapons);
        var elements = new FixedList64Bytes<byte>();
        var counts = new FixedList32Bytes<byte>();

        for (int i = 0; i < weaponCount; i++)
        {
            var weapon = (AutoAttack.WeaponType)i;
            int count = _loadout.GetElements(weapon, out var first, out var second, out var third);
            counts.Add((byte)Mathf.Clamp(count, 0, ElementsPerWeapon));

            elements.Add((byte)first);
            elements.Add((byte)second);
            elements.Add((byte)third);
        }

        _elements.Value = elements;
        _counts.Value = counts;
    }

    private void ApplyToLoadout()
    {
        if (_loadout == null)
        {
            return;
        }

        var elements = _elements.Value;
        var counts = _counts.Value;

        int weaponCount = counts.Length > 0 ? counts.Length : Mathf.Min(GetWeaponCount(), MaxWeapons);
        for (int i = 0; i < weaponCount; i++)
        {
            int count = i < counts.Length ? counts[i] : 0;
            int baseIndex = i * ElementsPerWeapon;
            var first = baseIndex < elements.Length ? (ElementType)elements[baseIndex] : ElementType.None;
            var second = baseIndex + 1 < elements.Length ? (ElementType)elements[baseIndex + 1] : ElementType.None;
            var third = baseIndex + 2 < elements.Length ? (ElementType)elements[baseIndex + 2] : ElementType.None;
            _loadout.SetElements((AutoAttack.WeaponType)i, first, second, third, count, false);
        }
    }

    private static int GetWeaponCount()
    {
        return System.Enum.GetValues(typeof(AutoAttack.WeaponType)).Length;
    }
}
