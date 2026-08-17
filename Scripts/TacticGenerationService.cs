using UnityEngine;
using System.Linq;

public static class TacticGenerationService
{
    public static RunManager.TacticSaveData GenerateTactic(Region? region = null, bool byPassExclusivity = false)
    {
        int day = RunManager.Instance.Stats.CurrentDay;
        DayRarityEntry dist = RunManager.Instance.rarityDistributionTable.GetForDay(day);
        Rarity rolledRarity = RarityDistributionTable.RollRarity(dist);

        // 1. Get a tactic from the database
        TacticDefinition def = TacticDatabase.Instance.GetRandomTactic(rolledRarity, region, byPassExclusivity);

        if (def == null) return null;

        // 2. Check the player's inventory immediately to adjust the rarity BEFORE the UI sees it!
        Rarity finalRarity = rolledRarity;

        var existingTactic = RunManager.Instance.playerTactics.FirstOrDefault(p =>
            p.tacticData != null &&
            p.tacticData.definition == def);

        if (existingTactic != null)
        {
            finalRarity = existingTactic.tacticData.rarity;
        }

        return new RunManager.TacticSaveData
        {
            definition = def,
            rarity = finalRarity
        };
    }
}
