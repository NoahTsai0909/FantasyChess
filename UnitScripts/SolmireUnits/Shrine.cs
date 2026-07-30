using System.Collections.Generic;
using UnityEngine;

public class Shrine : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> targets = FindAdjacentAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = target,
                amount = stats.Heal,
                reason = "Shrine Heal",
                isCrit = abilityCrit
            }
        );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_heal]Heals[/c] all adjacent allies for [HEAL] {stats.Heal}.");
    }
}
