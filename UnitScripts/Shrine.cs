using System.Collections.Generic;
using UnityEngine;

public class Shrine : UnitInstance
{
    protected override void UseAbility()
    {

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
                reason = "Shrine Heal"
            }
        );
        }
        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Heals all adjacent allies for {stats.Heal}.");
    }
}
