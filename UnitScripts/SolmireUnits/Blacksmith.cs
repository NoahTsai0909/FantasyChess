using UnityEngine;
using System.Collections.Generic;

public class Blacksmith : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> targets = FindAllAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = target,
                amount = stats.Shield,
                reason = "Blacksmith Shield",
                isCrit = abilityCrit
            }
        );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_shield]Shield[/c] all allies for [SHIELD] {stats.Shield}.");
    }
}
