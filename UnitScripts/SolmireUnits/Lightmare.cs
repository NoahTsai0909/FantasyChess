using System.Collections.Generic;
using UnityEngine;

public class Lightmare : UnitInstance
{

    private int hasteModifier = 0;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Uncommon)
        {
            hasteModifier = 0;
        }
        if (CurrentRarity == Rarity.Rare)
        {
            hasteModifier = 1;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            hasteModifier = 2;
        }
        else
        {
            hasteModifier = 0;
        }
    }

    protected override int GetRarityAdjustedHaste()
    {
        return hasteModifier;
    }

    protected override void UseAbility()
    {

        List<UnitInstance> targets = FindSideAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyHaste,
                source = this,
                target = target,
                amount = stats.Haste,
                reason = "Lightmare Haste"
            }

            );
        }
        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Hastes side allies for {stats.Haste} seconds.");
    }
}
