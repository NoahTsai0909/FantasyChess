using System.Collections.Generic;
using UnityEngine;

public class IronmoonTrinity : UnitInstance
{

    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> allies = FindAllAllies();
        int shieldAmount = stats.Shield;
        if (allies.Count == 3)
        {
            shieldAmount *= 3;
        }
        foreach (UnitInstance ally in allies)
        {
            CombatManager.Instance.ExecuteAction(
                 new CombatAction
                 {
                     type = CombatActionType.Shield,
                     source = this,
                     target = ally,
                     amount = shieldAmount,
                     reason = "Ironmoon Ruin shield",
                     isCrit = abilityCrit
                 }
             );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_shield]Shield[/c] all allies for [SHIELD] {stats.Shield}. When there are exactly 3 allies, triple the shield instead ([SHIELD] {stats.Shield * 3}).");
    }
}
