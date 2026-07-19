using System.Collections.Generic;
using UnityEngine;

public class Bulwark : UnitInstance
{
    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        base.EnterCombat(grid, row, col, isPlayer);

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
        if (action.source.unitName == this.unitName) return;

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

    
    public override string GetActiveDescription()
    {
        return ($"When an ally is healed, shield it {stats.Shield}. When an ally is shielded, heal it {stats.Heal}. ");
    }
}
