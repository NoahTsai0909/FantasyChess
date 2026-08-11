using UnityEngine;
using System.Collections.Generic;

public class Blacksmith : UnitInstance
{
    private int allyCount = 3;

    protected override void UseAbility()
    {
        base.UseAbility();

        List<UnitInstance> targets = FindLowestHealthAllies(allyCount);

        // If no allies, stop here
        if (targets.Count == 0) return;

        // Loop through the list and execute the attack on each one
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Shield,
                    source = this,
                    target = target,
                    amount = stats.Shield,
                    reason = "Blacksmith shield",
                    isCrit = abilityCrit
                }
            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_shield]Shield[/c] up to {allyCount} lowest health allies for [SHIELD] {stats.Shield}.");
    }
}
