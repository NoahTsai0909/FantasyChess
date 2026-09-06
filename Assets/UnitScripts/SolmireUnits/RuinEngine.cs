using System.Collections.Generic;
using UnityEngine;
using static CombatEventBus;

public class RuinEngine : UnitInstance
{
    private int advanceBuff = 1;
    private int enemyCount = 3;

    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> targets = FindNearestEnemies(enemyCount);

        if (targets.Count == 0) return;

        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = stats.Attack,
                    reason = "Ruin Engine Attack",
                    isCrit = abilityCrit
                }
            );
        }
    }

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);
        CombatEventBus.OnActionResolved += HandleCombatAction;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleCombatAction;
    }

    protected override void HandleCombatAction(CombatAction action)
    {
        if ((action.type == CombatActionType.Damage) && (action.source.isPlayer == this.isPlayer))
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Advance,
                source = this,
                target = this,
                amount = advanceBuff,
                reason = "Ruin Engine Passive"
            }
        );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest {enemyCount} enemies for [ATK] {stats.Attack}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"When an ally [c_attack]damages[/c] an enemy, advance this {advanceBuff} second.");
    }
}
