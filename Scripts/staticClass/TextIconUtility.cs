using UnityEngine;

public static class TextIconUtility
{
    // These constants must match the exact names you typed in your TMP Sprite Asset
    private const string GoldIcon = "<sprite name=gold>";
    private const string AttackIcon = "<sprite name=attack>";
    private const string ShieldIcon = "<sprite name=shield>";
    private const string HealIcon = "<sprite name=heal>";
    private const string BurnIcon = "<sprite name=burn>";
    private const string HasteIcon = "<sprite name=haste>";
    private const string PoisonIcon = "<sprite name=poison>";
    private const string SlowIcon = "<sprite name=slow>";

    // Standardized formatters for stats
    public static string FormatGold(int amount)
    {
        return $"{GoldIcon} {amount}"; // Result: "[Gold Icon] 4"
    }

    public static string FormatAttack(int amount)
    {
        return $"{AttackIcon} {amount}";
    }

    public static string FormatShield(int amount)
    {
        return $"{ShieldIcon} {amount}";
    }

    public static string FormatBurn(int amount)
    {
        return $"{BurnIcon} {amount}";
    }

    public static string FormatHaste(int amount)
    {
        return $"{HasteIcon} {amount}";
    }

    public static string FormatPoison(int amount)
    {
        return $"{PoisonIcon} {amount}";
    }

    public static string FormatSlow(int amount)
    {
        return $"{SlowIcon} {amount}";
    }

    public static string ParseDescription(string rawDescription)
    {
        if (string.IsNullOrEmpty(rawDescription)) return "";

        string parsedText = rawDescription
            .Replace("[GOLD]", GoldIcon)
            .Replace("[ATK]", AttackIcon)
            .Replace("[SHIELD]", ShieldIcon) // Fixed from .Initials to .Replace
            .Replace("[HEAL]", HealIcon)
            .Replace("[BURN]", BurnIcon)
            .Replace("[HASTE]", HasteIcon)
            .Replace("[POISON]", PoisonIcon)
            .Replace("[SLOW]", SlowIcon);

        return parsedText;
    }
    // write Attack the nearest enemy for 10 [ATK]. Shields self for 8 [SHIELD].
}
