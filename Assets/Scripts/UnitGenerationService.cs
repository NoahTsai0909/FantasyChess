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

    public static List<UnitSaveData> GenerateShopUnits(int count, Region region, UnitTagFlags unitTags, int minProvision = 0, int maxProvision = -1, bool forceRarity = false, Rarity designatedRarity = Rarity.Common)
    {
        var result = new List<UnitSaveData>();
        var usedDefinitions = new HashSet<UnitDefinition>();

        for (int i = 0; i < count; i++)
        {
            var rolledRarity = forceRarity ? designatedRarity : RunManager.Instance.RollRarityForDay(RunManager.Instance.Stats.CurrentDay);
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

                if (candidate != null && !usedDefinitions.Contains(candidate))
                {
                    // 1. Get the lowest rarity the player owns (returns null if they don't own it)
                    Rarity? lowestOwnedRarity = GetOwnedUnitLowestRarity(candidate);

                    bool isValid = true;

                    if (lowestOwnedRarity.HasValue)
                    {
                        // Rule A: The lowest they own is Epic (meaning they ONLY own Epics). They cannot upgrade this unit anymore. Reject it.
                        if (lowestOwnedRarity.Value == Rarity.Epic)
                        {
                            isValid = false;
                        }
                        // Rule B: It's a Forced Rarity Shop, but the rarity they need to merge doesn't match what the shop is allowed to sell. Reject it.
                        else if (forceRarity && lowestOwnedRarity.Value != designatedRarity)
                        {
                            isValid = false;
                        }
                    }

                    if (isValid)
                    {
                        def = candidate;
                    }
                }
            }

            if (def != null)
            {
                usedDefinitions.Add(def);

                // 2. Set the final rarity
                Rarity finalRarity = rolledRarity;
                Rarity? lowestOwnedRarity = GetOwnedUnitLowestRarity(def);

                // Rule C: If they own the unit, universally override the shop's rolled rarity 
                // to match their lowest owned copy so they can merge it.
                if (lowestOwnedRarity.HasValue)
                {
                    finalRarity = lowestOwnedRarity.Value;
                    Debug.Log($"[Shop] Rarity Override! {def.unitName} locked to {finalRarity} to match player inventory.");
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
