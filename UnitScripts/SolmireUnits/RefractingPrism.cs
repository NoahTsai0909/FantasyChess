using UnityEngine;
using static CombatEventBus;

public class RefractingPrism : UnitInstance
{
    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        base.EnterCombat(grid, row, col, isPlayer);

        if (isPassive)
        {
            CombatEventBus.OnActionResolved += HandleActionResolved;
        }
    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    private void HandleActionResolved(CombatAction action)
    {

        if (action.type != CombatActionType.Heal && action.type != CombatActionType.Damage) return;
        if (action.target != this) return;
        if (action.type == CombatActionType.Damage && action.source.isPlayer != this.isPlayer) {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = action.source,
                    amount = action.amount,
                    reason = "Refracting Prism Passive",
                    isPassive = true
                }
            );
        }
        if (action.type == CombatActionType.Heal && action.source.isPlayer == this.isPlayer)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Heal,
                    source = this,
                    target = action.source,
                    amount = action.amount,
                    reason = "Refracting Prism Passive",
                    isPassive = true
                }
            );
        }


    }

    public override string GetAbilityDescription()
    {
        return ($"Passive: When this is damaged by an enemy, return the same damage. When this is healed by an ally, return the same healing.");
    }
}
