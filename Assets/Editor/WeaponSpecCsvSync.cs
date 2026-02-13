using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WeaponSpecCsvSync
{
    private const string CsvPath = "Docs/WeaponSpec_7Types.csv";
    private const string ConfigAssetPath = "Assets/Resources/GameConfig.asset";
    private const string LastSyncTicksKey = "WeaponSpecCsvSync.LastWriteTicksUtc";

    [InitializeOnLoadMethod]
    private static void AutoSyncOnEditorLoad()
    {
        TrySyncIfCsvChanged(showDialog: false);
    }

    [MenuItem("Tools/Balance/Sync Weapon Specs From CSV")]
    private static void SyncFromCsvMenu()
    {
        TrySyncIfCsvChanged(showDialog: true, force: true);
    }

    private static void TrySyncIfCsvChanged(bool showDialog, bool force = false)
    {
        if (!File.Exists(CsvPath))
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Weapon Spec Sync", $"CSV not found:\n{CsvPath}", "OK");
            }
            return;
        }

        long currentTicks = File.GetLastWriteTimeUtc(CsvPath).Ticks;
        if (!force && currentTicks == ReadLastSyncTicks())
        {
            return;
        }

        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigAssetPath);
        if (config == null)
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Weapon Spec Sync", $"GameConfig not found:\n{ConfigAssetPath}", "OK");
            }
            return;
        }

        string csv = File.ReadAllText(CsvPath);
        if (!TryApplyToConfig(config, csv, out string report))
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Weapon Spec Sync", report, "OK");
            }
            return;
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorPrefs.SetString(LastSyncTicksKey, currentTicks.ToString(CultureInfo.InvariantCulture));
        Debug.Log(report);
        if (showDialog)
        {
            EditorUtility.DisplayDialog("Weapon Spec Sync", report, "OK");
        }
    }

    private static bool TryApplyToConfig(GameConfig config, string csv, out string report)
    {
        report = "Weapon spec sync failed.";
        if (config == null)
        {
            report = "GameConfig is null.";
            return false;
        }

        var lines = csv.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (lines.Length < 2)
        {
            report = "CSV is empty.";
            return false;
        }

        var header = ParseCsvLine(lines[0]);
        var headerIndex = BuildHeaderIndex(header);

        if (!headerIndex.TryGetValue("영문명", out int englishNameCol))
        {
            report = "CSV header '영문명' not found.";
            return false;
        }

        float baseDamage = Mathf.Max(0.0001f, config.autoAttack.baseProjectileDamage);
        float baseFireInterval = Mathf.Max(0.0001f, config.autoAttack.baseFireInterval);
        int updated = 0;
        var unknownWeapons = new List<string>();

        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            string raw = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var cells = ParseCsvLine(raw);
            if (englishNameCol >= cells.Count)
            {
                continue;
            }

            string englishName = cells[englishNameCol].Trim();
            if (string.IsNullOrWhiteSpace(englishName))
            {
                continue;
            }

            var target = ResolveWeaponStats(config.game, englishName);
            if (target == null)
            {
                unknownWeapons.Add(englishName);
                continue;
            }

            // displayName
            if (!string.IsNullOrWhiteSpace(englishName))
            {
                target.displayName = englishName;
            }

            // damageMult = baseDamage(absolute) / autoAttack.baseProjectileDamage
            if (TryGetFloat(cells, headerIndex, "기본피해", out float baseDamageAbsolute))
            {
                target.damageMult = Mathf.Max(0.1f, baseDamageAbsolute / baseDamage);
            }

            // fireRateMult = attacksPerSecond * baseFireInterval
            if (TryGetFloat(cells, headerIndex, "공격속도", out float attacksPerSecond))
            {
                target.fireRateMult = Mathf.Max(0.1f, attacksPerSecond * baseFireInterval);
            }

            if (TryGetFloat(cells, headerIndex, "RangeMult", out float rangeMult))
            {
                target.rangeMult = Mathf.Max(0f, rangeMult);
            }

            if (TryGetFloat(cells, headerIndex, "AreaMult", out float areaMult))
            {
                target.areaMult = Mathf.Max(0f, areaMult);
            }

            if (TryGetInt(cells, headerIndex, "투사체수", out int projectileCount))
            {
                target.bonusProjectiles = Mathf.Max(0, projectileCount - 1);
            }

            if (TryGetFloat(cells, headerIndex, "넉백", out float knockbackDistance))
            {
                target.knockbackDistance = Mathf.Max(0f, knockbackDistance);
            }

            if (TryGetFloat(cells, headerIndex, "경직초", out float hitStunDuration))
            {
                target.hitStunDuration = Mathf.Max(0f, hitStunDuration);
            }

            updated++;
        }

        if (updated == 0)
        {
            report = "No weapon rows were updated from CSV.";
            return false;
        }

        if (unknownWeapons.Count > 0)
        {
            report = $"Synced {updated} rows. Unknown weapon keys: {string.Join(", ", unknownWeapons)}";
        }
        else
        {
            report = $"Synced {updated} weapon rows from {CsvPath}.";
        }

        return true;
    }

    private static Dictionary<string, int> BuildHeaderIndex(List<string> header)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Count; i++)
        {
            string key = header[i].Trim();
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (!map.ContainsKey(key))
            {
                map[key] = i;
            }
        }
        return map;
    }

    private static bool TryGetFloat(List<string> cells, Dictionary<string, int> header, string column, out float value)
    {
        value = 0f;
        if (!header.TryGetValue(column, out int col) || col < 0 || col >= cells.Count)
        {
            return false;
        }

        string text = cells[col].Trim();
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryGetInt(List<string> cells, Dictionary<string, int> header, string column, out int value)
    {
        value = 0;
        if (!header.TryGetValue(column, out int col) || col < 0 || col >= cells.Count)
        {
            return false;
        }

        string text = cells[col].Trim();
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static WeaponStatsData ResolveWeaponStats(GameSessionSettings settings, string englishName)
    {
        switch (englishName.Trim().ToLowerInvariant())
        {
            case "singleshot":
                return settings.singleShotStats;
            case "multishot":
                return settings.multiShotStats;
            case "piercingshot":
                return settings.piercingShotStats;
            case "aura":
                return settings.auraStats;
            case "homingshot":
                return settings.homingShotStats;
            case "grenade":
                return settings.grenadeStats;
            case "melee":
                return settings.meleeStats;
            default:
                return null;
        }
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        if (line == null)
        {
            return result;
        }

        bool inQuotes = false;
        var current = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Length = 0;
                continue;
            }

            current.Append(c);
        }

        result.Add(current.ToString());
        return result;
    }

    private static long ReadLastSyncTicks()
    {
        string raw = EditorPrefs.GetString(LastSyncTicksKey, "0");
        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks))
        {
            return ticks;
        }

        return 0L;
    }
}
