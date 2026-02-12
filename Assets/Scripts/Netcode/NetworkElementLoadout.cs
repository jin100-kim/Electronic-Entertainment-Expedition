using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ElementLoadout))]
public class NetworkElementLoadout : NetworkBehaviour
{
    private const int MaxWeapons = 20;
    private const int ElementsPerWeapon = 3;
    private const int CharsPerWeapon = 1 + ElementsPerWeapon; // [count][e1][e2][e3]

    private readonly NetworkVariable<FixedString128Bytes> _payload = new NetworkVariable<FixedString128Bytes>(
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

        _payload.OnValueChanged += OnPayloadChanged;

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
        _payload.OnValueChanged -= OnPayloadChanged;
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

    private void OnPayloadChanged(FixedString128Bytes previous, FixedString128Bytes next)
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
        var chars = new char[weaponCount * CharsPerWeapon];
        int writeIndex = 0;

        for (int i = 0; i < weaponCount; i++)
        {
            var weapon = (AutoAttack.WeaponType)i;
            int count = _loadout.GetElements(weapon, out var first, out var second, out var third);

            chars[writeIndex++] = ToNibbleChar(Mathf.Clamp(count, 0, ElementsPerWeapon));
            chars[writeIndex++] = ToNibbleChar((int)first);
            chars[writeIndex++] = ToNibbleChar((int)second);
            chars[writeIndex++] = ToNibbleChar((int)third);
        }

        _payload.Value = new FixedString128Bytes(new string(chars, 0, writeIndex));
    }

    private void ApplyToLoadout()
    {
        if (_loadout == null)
        {
            return;
        }

        string raw = _payload.Value.ToString();
        if (string.IsNullOrEmpty(raw))
        {
            return;
        }

        int weaponCount = Mathf.Min(GetWeaponCount(), MaxWeapons);
        int expectedChars = weaponCount * CharsPerWeapon;
        if (raw.Length < expectedChars)
        {
            return;
        }

        for (int i = 0; i < weaponCount; i++)
        {
            int baseIndex = i * CharsPerWeapon;
            int count = FromNibbleChar(raw[baseIndex + 0]);
            int firstRaw = FromNibbleChar(raw[baseIndex + 1]);
            int secondRaw = FromNibbleChar(raw[baseIndex + 2]);
            int thirdRaw = FromNibbleChar(raw[baseIndex + 3]);

            var first = ToElementType(firstRaw);
            var second = ToElementType(secondRaw);
            var third = ToElementType(thirdRaw);
            _loadout.SetElements((AutoAttack.WeaponType)i, first, second, third, Mathf.Clamp(count, 0, ElementsPerWeapon), false);
        }
    }

    private static ElementType ToElementType(int value)
    {
        if (value < 0)
        {
            return ElementType.None;
        }

        if (!System.Enum.IsDefined(typeof(ElementType), value))
        {
            return ElementType.None;
        }

        return (ElementType)value;
    }

    private static char ToNibbleChar(int value)
    {
        int v = Mathf.Clamp(value, 0, 15);
        return (char)(v < 10 ? ('0' + v) : ('A' + (v - 10)));
    }

    private static int FromNibbleChar(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }
        if (c >= 'A' && c <= 'F')
        {
            return c - 'A' + 10;
        }
        if (c >= 'a' && c <= 'f')
        {
            return c - 'a' + 10;
        }

        return 0;
    }

    private static int GetWeaponCount()
    {
        return System.Enum.GetValues(typeof(AutoAttack.WeaponType)).Length;
    }
}
