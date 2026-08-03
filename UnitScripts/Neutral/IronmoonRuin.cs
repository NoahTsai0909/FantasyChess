using System.Collections.Generic;
using System.Security;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class IronmoonRuin : UnitInstance
{
    private int absorbedShield;
    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> allies = FindAllAllies();
        absorbedShield = 0;
        foreach(UnitInstance ally in allies)
        {
            if (ally.GetCurrentShield() > 0)
            {
                absorbedShield += ally.GetCurrentShield();
                ally.SetCurrentShield(0);
            }
        }

        CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Shield,
                    source = this,
                    target = this,
                    amount = absorbedShield,
                    reason = "Ironmoon Ruin shield",
                    isCrit = abilityCrit
                }
            );

        List<UnitInstance> enemies = FindAllEnemies();
        foreach(UnitInstance target in enemies)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = absorbedShield,
                    reason = "Ironmoon Ruin attack",
                    isCrit = abilityCrit
                }
            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"Absorb all ally [c_shield]shields[/c]. Attack all enemies for the absorbed amount.");
    }
}
