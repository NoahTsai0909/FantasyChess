using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GildedHelmet : UnitInstance
{
    private int enemyCount = 2;

    protected override void UseAbility()
    {
        base.UseAbility();
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = this,
                amount = stats.Shield,
                reason = "Gilded Helmet Shield",
                isCrit = abilityCrit
            }

         );

        List<UnitInstance> targets = FindNearestEnemies(enemyCount);
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = GetCurrentShield(),
                reason = "Gilded Helmet Damage",
                isCrit = abilityCrit
            }

            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_shield]Shield[/c] this for [SHIELD] {stats.Shield}. [c_attack]Attack[/c] up to {enemyCount} nearest enemies for the current amount of [c_shield]shield[/c] this has.");
    }
}
