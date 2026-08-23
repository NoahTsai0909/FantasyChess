using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ScorchingCaster : UnitInstance
{

    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindRandomEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyBurn,
                source = this,
                target = target,
                amount = stats.Burn,
                reason = "Scorching Caster Burn",
                isCrit = abilityCrit
            }
        );

    }

    public override string GetActiveDescription()
    {
        return ($"[c_burn]Burn[/c] a random enemy for [BURN]{stats.Burn}." );
    }


}
