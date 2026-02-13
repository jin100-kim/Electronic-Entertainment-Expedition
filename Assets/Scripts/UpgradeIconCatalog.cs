using System.Collections.Generic;
using UnityEngine;

public static class UpgradeIconCatalog
{
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    private const string IconWeaponSingleShot = "Art/Items/icon_weapon_single_shot";
    private const string IconWeaponMultiShot = "Art/Items/icon_weapon_multi_shot";
    private const string IconWeaponPiercingShot = "Art/Items/icon_weapon_piercing_shot";
    private const string IconWeaponAura = "Art/Items/icon_weapon_aura";
    private const string IconWeaponMelee = "Art/Items/icon_weapon_melee";

    private const string ProjectileHomingShot = "Art/Items/projectile_homing_shot";
    private const string ProjectileGrenade = "Art/Items/projectile_grenade";

    private const string IconStatAttackPower = "Art/Items/icon_stat_attack_power";
    private const string IconStatAttackSpeed = "Art/Items/icon_stat_attack_speed";
    private const string IconStatMoveSpeed = "Art/Items/icon_stat_move_speed";
    private const string IconStatHealthReinforce = "Art/Items/icon_stat_health_reinforce";
    private const string IconStatMagnet = "Art/Items/icon_stat_magnet";
    private const string IconStatAttackArea = "Art/Items/icon_stat_attack_area";
    private const string IconStatProjectileCount = "Art/Items/icon_stat_projectile_count";
    private const string IconStatPierce = "Art/Items/icon_stat_pierce";

    private const string PickupXp = "Art/Items/pickup_xp";
    private const string PickupCoin = "Art/Items/pickup_coin";
    private const string IconRewardHeal = "Art/Items/icon_reward_heal";

    public static Sprite ResolveSprite(string key, bool isWeapon)
    {
        string path = ResolvePath(key, isWeapon);
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (SpriteCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        var sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            var sprites = Resources.LoadAll<Sprite>(path);
            if (sprites != null && sprites.Length > 0)
            {
                sprite = sprites[0];
            }
        }

        SpriteCache[path] = sprite;
        return sprite;
    }

    public static bool TryResolveOptionTitle(string optionTitle, out Sprite sprite)
    {
        sprite = null;
        if (string.IsNullOrWhiteSpace(optionTitle))
        {
            return false;
        }

        string title = optionTitle.Trim();
        bool isWeapon = IsWeaponTitle(title, out string key);
        if (!isWeapon)
        {
            key = title;
        }

        sprite = ResolveSprite(key, isWeapon);
        return sprite != null;
    }

    private static bool IsWeaponTitle(string title, out string weaponKey)
    {
        weaponKey = title;
        if (title.StartsWith("Weapon:", System.StringComparison.OrdinalIgnoreCase))
        {
            weaponKey = title.Substring("Weapon:".Length).Trim();
            return true;
        }

        int separatorIndex = title.IndexOf(':');
        if (separatorIndex > 0)
        {
            string prefix = title.Substring(0, separatorIndex).Trim();
            if (prefix.IndexOf("\uBB34\uAE30", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                weaponKey = title.Substring(separatorIndex + 1).Trim();
                return true;
            }
        }

        string[] weaponTerms = { "SingleShot", "MultiShot", "PiercingShot", "Aura", "HomingShot", "Grenade", "Melee" };
        for (int i = 0; i < weaponTerms.Length; i++)
        {
            if (title.IndexOf(weaponTerms[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                weaponKey = weaponTerms[i];
                return true;
            }
        }

        return false;
    }

    private static string ResolvePath(string key, bool isWeapon)
    {
        string normalized = key ?? string.Empty;

        if (isWeapon)
        {
            if (ContainsAny(normalized, "SingleShot", "single"))
            {
                return IconWeaponSingleShot;
            }
            if (ContainsAny(normalized, "MultiShot", "boomerang", "boom"))
            {
                return IconWeaponMultiShot;
            }
            if (ContainsAny(normalized, "PiercingShot", "pierce", "nova"))
            {
                return IconWeaponPiercingShot;
            }
            if (ContainsAny(normalized, "Aura", "shotgun"))
            {
                return IconWeaponAura;
            }
            if (ContainsAny(normalized, "HomingShot", "homing", "shuriken"))
            {
                return ProjectileHomingShot;
            }
            if (ContainsAny(normalized, "Grenade", "grenade", "frost"))
            {
                return ProjectileGrenade;
            }
            if (ContainsAny(normalized, "Melee", "melee"))
            {
                return IconWeaponMelee;
            }

            return IconWeaponSingleShot;
        }

        // Fixed rewards
        if (ContainsAny(normalized, "HP 회복", "회복", "heal"))
        {
            return IconRewardHeal;
        }
        if (ContainsAny(normalized, "코인", "coin"))
        {
            return PickupCoin;
        }

        // Stats
        if (ContainsAny(normalized, "\uACF5\uACA9\uB825", "damage"))
        {
            return IconStatAttackPower;
        }
        if (ContainsAny(normalized, "\uACF5\uACA9\uC18D\uB3C4", "fire", "rate"))
        {
            return IconStatAttackSpeed;
        }
        if (ContainsAny(normalized, "\uC774\uB3D9\uC18D\uB3C4", "move", "speed"))
        {
            return IconStatMoveSpeed;
        }
        if (ContainsAny(normalized, "\uCCB4\uB825\uAC15\uD654", "\uCCB4\uB825 \uAC15\uD654", "\uCCB4\uB825", "health", "regen"))
        {
            return IconStatHealthReinforce;
        }
        if (ContainsAny(normalized, "\uC0AC\uAC70\uB9AC", "range"))
        {
            return IconWeaponSingleShot;
        }
        // IMPORTANT: magnet must be checked before xp to avoid "경험치 자석" mapping to xp icon
        if (ContainsAny(normalized, "\uACBD\uD5D8\uCE58 \uC790\uC11D", "\uACBD\uD5D8\uCE58\uC790\uC11D", "\uC790\uC11D", "magnet", "xp magnet"))
        {
            return IconStatMagnet;
        }
        if (ContainsAny(normalized, "\uACBD\uD5D8\uCE58", "xp"))
        {
            return PickupXp;
        }
        if (ContainsAny(normalized, "\uACF5\uACA9\uBC94\uC704", "area", "size"))
        {
            return IconStatAttackArea;
        }
        if (ContainsAny(normalized, "\uD22C\uC0AC\uCCB4 \uC218", "\uD22C\uC0AC\uCCB4\uC218", "projectile", "count"))
        {
            return IconStatProjectileCount;
        }
        if (ContainsAny(normalized, "\uAD00\uD1B5", "pierce"))
        {
            return IconStatPierce;
        }

        return PickupXp;
    }

    private static bool ContainsAny(string source, params string[] terms)
    {
        if (string.IsNullOrEmpty(source) || terms == null)
        {
            return false;
        }

        for (int i = 0; i < terms.Length; i++)
        {
            var term = terms[i];
            if (string.IsNullOrWhiteSpace(term))
            {
                continue;
            }

            if (source.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
