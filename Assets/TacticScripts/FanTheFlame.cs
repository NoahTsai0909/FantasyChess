using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FanTheFlame : TacticInstance
{
    public int hasteStacks = 1;

    // Calculate the haste stacks based on the tier (1/2/3/4)
    private int GetHasteStacks(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 1,
            Rarity.Uncommon => 2,
            Rarity.Rare => 3,
            Rarity.Epic => 4,
            _ => 1
        };
    }

    public override void InitializeFromSaveData(RunManager.TacticSaveData data)
    {
        base.InitializeFromSaveData(data);
        hasteStacks = GetHasteStacks(CurrentRarity);
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        hasteStacks = GetHasteStacks(CurrentRarity);
    }

    public override void ExecuteActiveEffect()
    {
        base.ExecuteActiveEffect();

        List<UnitInstance> allies = FindAllAllies();

        List<UnitInstance> burnAllies = allies.Where(unit =>
            unit != null &&
            unit.Definition != null &&
            unit.Stats.Tags.HasFlag(UnitTagFlags.Burn)).ToList();

        if (burnAllies.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, burnAllies.Count);
            UnitInstance target = burnAllies[randomIndex];

            // Route through CombatManager exactly like a Unit does!
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ExecuteAction(new CombatAction
                {
                    type = CombatActionType.ApplyHaste,
                    source = null, // Null because Tactics aren't physical units!
                    target = target,
                    amount = hasteStacks,
                    reason = tacticName
                });
            }

            Debug.Log($"<color=orange>{tacticName}</color> fired! Queued {hasteStacks} Haste for {target.unitName}.");
        }
        else
        {
            Debug.Log($"<color=orange>{tacticName}</color> fired, but no Burn units were found on the board!");
        }
    }


    public override string GetDescription()
    {
        return ($"[c_haste]Haste[/c] a random [c_burn]burn[/c] ally for [HASTE] {hasteStacks}.");
    }
}


