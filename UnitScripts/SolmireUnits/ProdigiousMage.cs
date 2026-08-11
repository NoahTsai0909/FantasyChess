using System.Collections.Generic;
using UnityEngine;

public class ProdigiousMage : UnitInstance
{
    private int enemyCount = 2;
    protected override void UseAbility()
    {
        base.UseAbility();

        List<UnitInstance> targets = FindFarthestEnemies(enemyCount);
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
                    reason = "Prodigious Mage Attack",
                    isCrit = abilityCrit
                }
            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] up to {enemyCount} farthest enemies for [ATK] {stats.Attack}.");
    }
}
