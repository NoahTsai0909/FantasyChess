using UnityEngine;
using static CombatEventBus;

public class RefractingPrism : UnitInstance
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
        if (action.source == null) return;
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
        if (currentSuffix != null)
        {
            currentSuffix.ExecuteEffect(this);
        }

    }


    public override string GetPassiveDescription()
    {
        return ($"When this is [c_attack]attacked[/c] by an enemy, return the same [c_attack]damage[/c]. When this is [c_heal]healed[/c] by an ally, return the same [c_heal]healing[/c].");
    }

    public override string GetMutationTriggerText()
    {
        return ("<br>Also ");
    }
}
