using UnityEngine;

public class IronmoonCocoon : UnitInstance
{
    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
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
        if (action.type != CombatActionType.Shield || action.target != this || action.source == this) return;
        CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Shield,
                    source = this,
                    target = this,
                    amount = stats.Shield,
                    reason = "Ironmoon Cocoon Shield"
                }
            );
    }

    public override string GetPassiveDescription()
    {
        return ($"When this is [c_shield]shielded[/c], [c_shield]shield[/c] this for [SHIELD] {stats.Shield}.");
    }
}
