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
        UnitTagFlags requiredTags = UnitTagFlags.None,
        bool bypassExclusive = false)
    {
        int day = RunManager.Instance.Stats.CurrentDay;
        DayRarityEntry dist = RunManager.Instance.rarityDistributionTable.GetForDay(day);
        Rarity rolledRarity = RarityDistributionTable.RollRarity(dist);
        UnitDefinition def = UnitDatabase.Instance.GetRandomUnit(rolledRarity, region, requiredTags, 0, -1, bypassExclusive);

        return new UnitSaveData
        {
            definition = def,
            rarity = rolledRarity
        };
    }

    public static List<UnitSaveData> GenerateShopUnits(int count, Region region, UnitTagFlags unitTags, int minProvision = 0, int maxProvision = -1)
    {
        var result = new List<UnitSaveData>();
        var usedDefinitions = new HashSet<UnitDefinition>();

        for (int i = 0; i < count; i++)
        {
            var rolledRarity = RunManager.Instance.RollRarityForDay(RunManager.Instance.Stats.CurrentDay);
            UnitDefinition def = null;
            int attempts = 0;

            while (def == null && attempts < 100)
            {
                attempts++;

                var candidate = UnitDatabase.Instance.GetRandomUnit(
                    rolledRarity,
                    region,
                    unitTags,
                    minProvision,
                    maxProvision
                );

                if (candidate != null && !usedDefinitions.Contains(candidate) && !IsMaxRarityOwned(candidate))
                {
                    def = candidate;
                }
            }

            if (def != null)
            {
                usedDefinitions.Add(def);

                Rarity finalRarity = rolledRarity;
                Rarity? ownedLowestRarity = GetOwnedUnitLowestRarity(def);

                if (ownedLowestRarity.HasValue)
                {
                    finalRarity = ownedLowestRarity.Value;
                    Debug.Log($"[Shop] Rarity Override! {def.unitName} changed to {finalRarity} to match player inventory.");
                }

                result.Add(new UnitSaveData
                {
                    definition = def,
                    rarity = finalRarity
                });
            }
        }

        return result;
    }

    private static Rarity? GetOwnedUnitLowestRarity(UnitDefinition targetDef)
    {
        Rarity? lowestRarity = null;

        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (placement.unitData != null && placement.unitData.definition == targetDef)
            {
                if (lowestRarity == null || placement.unitData.rarity < lowestRarity.Value)
                    lowestRarity = placement.unitData.rarity;
            }
        }

        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            if (placement.unitData != null && placement.unitData.definition == targetDef)
            {
                if (lowestRarity == null || placement.unitData.rarity < lowestRarity.Value)
                    lowestRarity = placement.unitData.rarity;
            }
        }

        return lowestRarity;
    }

    private static bool IsMaxRarityOwned(UnitDefinition targetDef)
    {
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (placement.unitData != null && placement.unitData.definition == targetDef && placement.unitData.rarity == Rarity.Epic)
                return true;
        }

        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            if (placement.unitData != null && placement.unitData.definition == targetDef && placement.unitData.rarity == Rarity.Epic)
                return true;
        }

        return false;
    }
}
