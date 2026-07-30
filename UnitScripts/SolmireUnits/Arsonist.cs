using UnityEngine;

public class Arsonist : UnitInstance
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

        if (action.type != CombatActionType.ApplyBurn) return;
        if (action.target.isPlayer == this.isPlayer) return;
        if (action.isPassive) return;
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.ApplyBurn,
                    source = this,
                    target = action.target,
                    amount = stats.Burn,
                    reason = "Arsonist Passive",
                    isPassive = true
                }
            );
        }

    }

    public override string GetPassiveDescription()
    {
        return ($"When an enemy is applied [c_burn]burn[/c] from an ability, [c_burn]burn[/c] it for [BURN] {stats.Burn}.");
    }
}
