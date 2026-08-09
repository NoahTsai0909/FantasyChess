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
    public static string FormatGold(int amount) => $"<nobr><link=\"gold\">{GoldIcon} <color={ColorGold}>{amount}</color></link></nobr>";
    public static string FormatAttack(int amount) => $"<nobr><link=\"attack\">{AttackIcon} <color={ColorAttack}>{amount}</color></link></nobr>";
    public static string FormatShield(int amount) => $"<nobr><link=\"shield\">{ShieldIcon} <color={ColorShield}>{amount}</color></link></nobr>";
    public static string FormatBurn(int amount) => $"<nobr><link=\"burn\">{BurnIcon} <color={ColorBurn}>{amount}</color></link></nobr>";
    public static string FormatHaste(int amount) => $"<nobr><link=\"haste\">{HasteIcon} <color={ColorHaste}>{amount}</color></link></nobr>";
    public static string FormatPoison(int amount) => $"<nobr><link=\"poison\">{PoisonIcon} <color={ColorPoison}>{amount}</color></link></nobr>";
    public static string FormatSlow(int amount) => $"<nobr><link=\"slow\">{SlowIcon} <color={ColorSlow}>{amount}</color></link></nobr>";
    public static string FormatHeal(int amount) => $"<nobr><link=\"heal\">{HealIcon} <color={ColorHeal}>{amount}</color></link></nobr>";
    public static string FormatEnergy(int amount) => $"<nobr><link=\"energy\">{EnergyIcon} <color={ColorEnergy}>{amount}</color></link></nobr>";
    public static string FormatCrit(int amount) => $"<nobr><link=\"crit\">{CritIcon} <color={ColorCrit}>{amount}</color></link></nobr>";
    public static string FormatMaxHealth(int amount) => $"<nobr><link=\"maxhealth\">{MaxHealthIcon} <color={ColorMaxHealth}>{amount}</color></link></nobr>";
    public static string FormatMulticast(int amount) => $"<nobr><link=\"multicast\">{MulticastIcon} <color={ColorMulticast}>{amount}</color></link></nobr>";
    public static string FormatProvision(int amount) => $"<nobr><link=\"provision\">{ProvisionIcon} <color={ColorProvision}>{amount}</color></link></nobr>";
    public static string FormatMaxProvision(int amount) => $"<nobr><link=\"maxprovision\">{MaxProvisionIcon} <color={ColorMaxProvision}>{amount}</color></link></nobr>";
    public static string ParseDescription(string rawDescription)
    {
        if (string.IsNullOrEmpty(rawDescription)) return "";

        string parsedText = rawDescription;

        // 1. Update the Regex to wrap the icon and number in a <link> tag
        parsedText = Regex.Replace(parsedText, @"\[GOLD\]\s*(\d+)", $"<nobr><link=\"gold\">{GoldIcon} <color={ColorGold}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[ATK\]\s*(\d+)", $"<nobr><link=\"attack\">{AttackIcon} <color={ColorAttack}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[SHIELD\]\s*(\d+)", $"<nobr><link=\"shield\">{ShieldIcon} <color={ColorShield}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[HEAL\]\s*(\d+)", $"<nobr><link=\"heal\">{HealIcon} <color={ColorHeal}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[BURN\]\s*(\d+)", $"<nobr><link=\"burn\">{BurnIcon} <color={ColorBurn}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[HASTE\]\s*(\d+)", $"<nobr><link=\"haste\">{HasteIcon} <color={ColorHaste}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[POISON\]\s*(\d+)", $"<nobr><link=\"poison\">{PoisonIcon} <color={ColorPoison}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[SLOW\]\s*(\d+)", $"<nobr><link=\"slow\">{SlowIcon} <color={ColorSlow}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[MAXHEALTH\]\s*(\d+)", $"<nobr><link=\"maxhealth\">{MaxHealthIcon} <color={ColorMaxHealth}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[ENERGY\]\s*(\d+)", $"<nobr><link=\"energy\">{EnergyIcon} <color={ColorEnergy}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[CRIT\]\s*(\d+)", $"<nobr><link=\"crit\">{CritIcon} <color={ColorCrit}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[MULTICAST\]\s*(\d+)", $"<nobr><link=\"multicast\">{MulticastIcon} <color={ColorMulticast}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[PROVISION\]\s*(\d+)", $"<nobr><link=\"provision\">{ProvisionIcon} <color={ColorProvision}>$1</color></link></nobr>");
        parsedText = Regex.Replace(parsedText, @"\[MAXPROVISION\]\s*(\d+)", $"<nobr><link=\"maxprovision\">{MaxProvisionIcon} <color={ColorMaxProvision}>$1</color></link></nobr>");

        // 2. Wrap standalone icons in links
        parsedText = parsedText.Replace("[GOLD]", $"<link=\"gold\">{GoldIcon}</link>")
                               .Replace("[ATK]", $"<link=\"attack\">{AttackIcon}</link>")
                               .Replace("[SHIELD]", $"<link=\"shield\">{ShieldIcon}</link>")
                               .Replace("[HEAL]", $"<link=\"heal\">{HealIcon}</link>")
                               .Replace("[BURN]", $"<link=\"burn\">{BurnIcon}</link>")
                               .Replace("[HASTE]", $"<link=\"haste\">{HasteIcon}</link>")
                               .Replace("[POISON]", $"<link=\"poison\">{PoisonIcon}</link>")
                               .Replace("[SLOW]", $"<link=\"slow\">{SlowIcon}</link>")
                               .Replace("[ENERGY]", $"<link=\"energy\">{EnergyIcon}</link>")
                               .Replace("[CRIT]", $"<link=\"crit\">{CritIcon}</link>")
                               .Replace("[MULTICAST]", $"<link=\"multicast\">{MulticastIcon}</link>")
                               .Replace("[PROVISION]", $"<link=\"provision\">{ProvisionIcon}</link>")
                               .Replace("[MAXPROVISION]", $"<link=\"maxprovision\">{MaxProvisionIcon}</link>")
                               .Replace("[MAXHEALTH]", $"<link=\"maxhealth\">{MaxHealthIcon}</link>");

        // 3. Inject the link tag alongside the color tag for text keywords
        parsedText = parsedText.Replace("[c_attack]", $"<link=\"attack\"><color={ColorAttack}>")
                               .Replace("[c_shield]", $"<link=\"shield\"><color={ColorShield}>")
                               .Replace("[c_heal]", $"<link=\"heal\"><color={ColorHeal}>")
                               .Replace("[c_gold]", $"<link=\"gold\"><color={ColorGold}>")
                               .Replace("[c_burn]", $"<link=\"burn\"><color={ColorBurn}>")
                               .Replace("[c_haste]", $"<link=\"haste\"><color={ColorHaste}>")
                               .Replace("[c_poison]", $"<link=\"poison\"><color={ColorPoison}>")
                               .Replace("[c_slow]", $"<link=\"slow\"><color={ColorSlow}>")
                               .Replace("[c_maxhealth]", $"<link=\"maxhealth\"><color={ColorMaxHealth}>")
                               .Replace("[c_energy]", $"<link=\"energy\"><color={ColorEnergy}>")
                               .Replace("[c_crit]", $"<link=\"crit\"><color={ColorCrit}>")
                               .Replace("[c_multicast]", $"<link=\"multicast\"><color={ColorMulticast}>")
                               .Replace("[c_provision]", $"<link=\"provision\"><color={ColorProvision}>")
                               .Replace("[c_maxprovision]", $"<link=\"maxprovision\"><color={ColorMaxProvision}>")

                               // 4. Close BOTH the color and the link tag
                               .Replace("[/c]", "</color></link>");

        return parsedText;
    }
}
