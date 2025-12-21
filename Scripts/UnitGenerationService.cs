using UnityEngine;
public static class UnitGenerationService
{
    /// <summary>
    /// Core unit generation entry point.
    /// All unit generation (shop, events, rewards) should go through this.
    /// </summary>
    public static UnitSaveData GenerateUnit(
        Region? region = null,
        UnitTagFlags requiredTags = UnitTagFlags.None)
    {
        // Safety checks
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager.Instance is null");
            return null;
        }

        if (RunManager.Instance.rarityDistributionTable == null)
        {
            Debug.LogError("RarityDistributionTable not assigned in RunManager");
            return null;
        }

        // Get current day
        int day = RunManager.Instance.currentDay;

        // Get rarity distribution for this day
        DayRarityEntry dist = RunManager.Instance.rarityDistributionTable.GetForDay(day);

        if (dist == null)
        {
            Debug.LogError($"No rarity distribution found for day {day}");
            return null;
        }

        // Roll rarity
        Rarity rolledRarity = RarityDistributionTable.RollRarity(dist);

        // Get eligible unit
        UnitDefinition def = UnitDatabase.Instance.GetRandomUnit(rolledRarity, region, requiredTags);

        if (def == null)
        {
            Debug.LogWarning(
                $"No unit found for rarity {rolledRarity}, region {region}, tags {requiredTags}"
            );
            return null;
        }

        // Return full unit data
        return new UnitSaveData
        {
            definition = def,
            rarity = rolledRarity
        };
    }
}
