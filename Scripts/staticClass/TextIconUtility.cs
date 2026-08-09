using UnityEngine;
using System.Text.RegularExpressions; // Needed for Regex

public static class TextIconUtility
{
    private const string GoldIcon = "<sprite name=gold>";
    private const string AttackIcon = "<sprite name=attack>";
    private const string ShieldIcon = "<sprite name=shield>";
    private const string HealIcon = "<sprite name=heal>";
    private const string BurnIcon = "<sprite name=burn>";
    private const string HasteIcon = "<sprite name=haste>";
    private const string PoisonIcon = "<sprite name=poison>";
    private const string SlowIcon = "<sprite name=slow>";
    private const string CritIcon = "<sprite name=crit>";
    private const string EnergyIcon = "<sprite name=energy>";
    private const string MulticastIcon = "<sprite name=multicast>";
    private const string ProvisionIcon = "<sprite name=provision>";
    private const string MaxProvisionIcon = "<sprite name=maxprovision>";
    private const string MaxHealthIcon = "<sprite name=maxhealth>";

    // Define your hex colors
    private const string ColorGold = "#FFFFFF";
    private const string ColorAttack = "#FF4444";
    private const string ColorShield = "#AD7100";
    private const string ColorHeal = "#02E302";
    private const string ColorBurn = "#FF8800";
    private const string ColorHaste = "#66FFE7";
    private const string ColorPoison = "#9933CC";
    private const string ColorSlow = "#8C6633";
    private const string ColorEnergy = "#EF1DAA";
    private const string ColorCrit = "#FE8500";
    private const string ColorMulticast = "#FF4444";
    private const string ColorProvision = "#AA55CB";
    private const string ColorMaxProvision = "#AA55CB";
    private const string ColorMaxHealth = "#088A06";

    // Standardized formatters for stats (useful for UI elements like price tags)
    public static string FormatGold(int amount) => $"<nobr>{GoldIcon} <color={ColorGold}>{amount}</color></nobr>";
    public static string FormatAttack(int amount) => $"<nobr>{AttackIcon} <color={ColorAttack}>{amount}</color></nobr>";
    public static string FormatShield(int amount) => $"<nobr>{ShieldIcon} <color={ColorShield}>{amount}</color></nobr>";
    public static string FormatBurn(int amount) => $"<nobr>{BurnIcon} <color={ColorBurn}>{amount}</color></nobr>";
    public static string FormatHaste(int amount) => $"<nobr>{HasteIcon} <color={ColorHaste}>{amount}</color></nobr>";
    public static string FormatPoison(int amount) => $"<nobr>{PoisonIcon} <color={ColorPoison}>{amount}</color></nobr>";
    public static string FormatSlow(int amount) => $"<nobr>{SlowIcon} <color={ColorSlow}>{amount}</color></nobr>";

    public static string FormatHeal(int amount) => $"<nobr>{HealIcon} <color={ColorHeal}>{amount}</color></nobr>";

    public static string FormatEnergy(int amount) => $"<nobr>{EnergyIcon} <color={ColorEnergy}>{amount}</color></nobr>";

    public static string FormatCrit(int amount) => $"<nobr>{CritIcon} <color={ColorCrit}>{amount}</color></nobr>";

    public static string FormatMaxHealth(int amount) => $"<nobr>{MaxHealthIcon} <color={ColorMaxHealth}>{amount}</color></nobr>";   
    public static string FormatMulticast(int amount) => $"<nobr>{MulticastIcon} <color={ColorMulticast}>{amount}</color></nobr>";

    public static string FormatProvision(int amount) => $"<nobr>{ProvisionIcon} <color={ColorProvision}>{amount}</color></nobr>";

    public static string FormatMaxProvision(int amount) => $"<nobr>{MaxProvisionIcon} <color={ColorMaxProvision}>{amount}</color></nobr>";
    public static string ParseDescription(string rawDescription)
    {
        if (string.IsNullOrEmpty(rawDescription)) return "";

        string parsedText = rawDescription;

        // How Regex works here:
        // @"\[ATK\]\s*(\d+)" looks for "[ATK]", any amount of space, and then a number.
        // It replaces it with the icon, the color tag, the number ($1), and the closing color tag.
        parsedText = Regex.Replace(parsedText, @"\[GOLD\]\s*(\d+)", $"<nobr>{GoldIcon} <color={ColorGold}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[ATK\]\s*(\d+)", $"<nobr>{AttackIcon} <color={ColorAttack}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[SHIELD\]\s*(\d+)", $"<nobr>{ShieldIcon} <color={ColorShield}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[HEAL\]\s*(\d+)", $"<nobr>{HealIcon} <color={ColorHeal}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[BURN\]\s*(\d+)", $"<nobr>{BurnIcon} <color={ColorBurn}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[HASTE\]\s*(\d+)", $"<nobr>{HasteIcon} <color={ColorHaste}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[POISON\]\s*(\d+)", $"<nobr>{PoisonIcon} <color={ColorPoison}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[SLOW\]\s*(\d+)", $"<nobr>{SlowIcon} <color={ColorSlow}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[MAXHEALTH\]\s*(\d+)", $"<nobr>{MaxHealthIcon} <color={ColorMaxHealth}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[ENERGY\]\s*(\d+)", $"<nobr>{EnergyIcon} <color={ColorEnergy}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[CRIT\]\s*(\d+)", $"<nobr>{CritIcon} <color={ColorCrit}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[MULTICAST\]\s*(\d+)", $"<nobr>{MulticastIcon} <color={ColorMulticast}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[PROVISION\]\s*(\d+)", $"<nobr>{ProvisionIcon} <color={ColorProvision}>$1</color></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[MAXPROVISION\]\s*(\d+)", $"<nobr>{MaxProvisionIcon} <color={ColorMaxProvision}>$1</color></nobr>");

        // Fallback: If you just type [ATK] without a number after it, just replace the icon
        parsedText = parsedText.Replace("[GOLD]", GoldIcon)
                               .Replace("[ATK]", AttackIcon)
                               .Replace("[SHIELD]", ShieldIcon)
                               .Replace("[HEAL]", HealIcon)
                               .Replace("[BURN]", BurnIcon)
                               .Replace("[HASTE]", HasteIcon)
                               .Replace("[POISON]", PoisonIcon)
                               .Replace("[SLOW]", SlowIcon)
                               .Replace("[ENERGY]", EnergyIcon)
                               .Replace("[CRIT]", CritIcon)
                               .Replace("[MULTICAST]", MulticastIcon)
                               .Replace("[PROVISION]", ProvisionIcon)
                               .Replace("[MAXPROVISION]", MaxProvisionIcon)
                               .Replace("[MAXHEALTH]", MaxHealthIcon);
        

        parsedText = parsedText.Replace("[c_attack]", $"<color={ColorAttack}>")
                               .Replace("[c_shield]", $"<color={ColorShield}>")
                               .Replace("[c_heal]", $"<color={ColorHeal}>")
                               .Replace("[c_gold]", $"<color={ColorGold}>")
                                 .Replace("[c_burn]", $"<color={ColorBurn}>")
                                 .Replace("[c_haste]", $"<color={ColorHaste}>")
                                 .Replace("[c_poison]", $"<color={ColorPoison}>")
                                 .Replace("[c_slow]", $"<color={ColorSlow}>")
                                 .Replace("[c_maxhealth]", $"<color={ColorMaxHealth}>")
                                 .Replace("[c_energy]", $"<color={ColorEnergy}>")
                                 .Replace("[c_crit]", $"<color={ColorCrit}>")
                                 .Replace("[c_multicast]", $"<color={ColorMulticast}>")
                                 .Replace("[c_provision]", $"<color={ColorProvision}>")
                                 .Replace("[c_maxprovision]", $"<color={ColorMaxProvision}>")
                               .Replace("[/c]", "</color>");

        return parsedText;
    }
}
