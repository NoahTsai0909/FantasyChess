using System.Collections.Generic;
using System.Linq;
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
        // Get current day
        int day = RunManager.Instance.currentDay;

        // Get rarity distribution for this day
        DayRarityEntry dist = RunManager.Instance.rarityDistributionTable.GetForDay(day);

        // Roll rarity
        Rarity rolledRarity = RarityDistributionTable.RollRarity(dist);

        // Get eligible unit (your existing logic)
        UnitDefinition def = UnitDatabase.Instance.GetRandomUnit(rolledRarity, region, requiredTags);

        return new UnitSaveData
        {
            definition = def,
            rarity = rolledRarity
        };
    }


}
