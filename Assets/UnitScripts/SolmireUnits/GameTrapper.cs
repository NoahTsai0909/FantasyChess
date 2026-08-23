using System.Collections.Generic;
using UnityEngine;

public class GameTrapper : UnitInstance
{
    private int enemyCount = 2;
    protected override void UseAbility()
    {
        base.UseAbility();

        List<UnitInstance> targets = FindNearestEnemies(enemyCount);

        // If no enemies are left, stop here
        if (targets.Count == 0) return;

        // Loop through the list and execute the attack on each one
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = stats.Attack,
                    reason = "Trapper Attack",
                    isCrit = abilityCrit
                }
            );
        }

        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.ApplySlow,
                    source = this,
                    target = target,
                    amount = stats.Slow,
                    reason = "Trapper Slow"
                }
            );
        }
    }
    public override string GetActiveDescription()
    {
        return $"[c_attack]Attack[/c] up to {enemyCount} nearest enemies for [ATK] {stats.Attack}. [c_slow]Slow[/c] each for [SLOW] {stats.Slow}.";
    }
}
