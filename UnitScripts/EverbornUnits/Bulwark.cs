using System.Collections.Generic;
using UnityEngine;

public class Bulwark : UnitInstance
{
    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);

        CombatEventBus.OnActionResolved += HandleActionResolved;

    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    private void HandleActionResolved(CombatAction action)
    {

        if (action.type != CombatActionType.Heal && action.type != CombatActionType.Shield) return;
        if (action.target.isPlayer != this.isPlayer) return;
        if (action.source == this) return;

        if (action.type == CombatActionType.Heal)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = action.target,
                amount = stats.Shield,
                reason = "Bulwark Shield Passive",
                isPassive = true
            }
        );
        }
        else if (action.type == CombatActionType.Shield)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = action.target,
                amount = stats.Heal,
                reason = "Bulwark Heal Passive",
                isPassive = true
            }
        );
    }
    }


    public override string GetPassiveDescription()
    {
        return ($"When an ally is [c_heal]healed[/c], [c_shield]shield[/c] it for [SHIELD] {stats.Shield}. When an ally is [c_shield]shielded[/c], [c_heal]heal[/c] it for [HEAL] {stats.Heal}. ");
    }
}
