using System.Collections.Generic;
using UnityEngine;

public class Dustraiser : UnitInstance
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
        base.UseAbility();
        List<UnitInstance> targets = FindAllAllies();
        foreach (UnitInstance target in targets)
        {
            bool sameRow = target.row == row;
            bool behind;
            if ( isPlayer)
            {
                behind = target.col == col - 1;
            }
            else
            {
                behind = target.col == col + 1;
            }
            if (sameRow && behind)
            {
                CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.ApplyHaste,
                    source = this,
                    target = target,
                    amount = stats.Haste,
                    reason = "DustRaiser Haste"
                }

                );
                CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Shield,
                    source = this,
                    target = target,
                    amount = stats.Shield,
                    reason = "DustRaiser Shield",
                    isCrit = abilityCrit
                }

                );
            }

        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_haste]Haste[/c] the ally behind this for [HASTE] {stats.Haste} and [c_shield]shield[/c] it for [SHIELD] {stats.Shield}.");
    }
}
