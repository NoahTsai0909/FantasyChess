using System.Collections.Generic;
using UnityEngine;

public class Lightmare : UnitInstance
{
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
            Debug.Log($"Lightmare hasted {target.unitName}");
        }
        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Hastes side allies for {stats.Haste} seconds.");
    }
}
