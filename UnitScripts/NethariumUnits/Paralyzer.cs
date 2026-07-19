using System.Collections.Generic;
using UnityEngine;

public class Paralyzer : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();

        UnitInstance target = FindNearestEnemy();

        // If no enemies are left, stop here
        if (target == null) return;


        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplySlow,
                source = this,
                target = target,
                amount = stats.Slow,
                reason = "Paralyzer Slow"
            }
        );
        
    }

    public override string GetActiveDescription()
    {
        return ($"Slow the nearest enemy for {stats.Slow} seconds.");
    }
}
