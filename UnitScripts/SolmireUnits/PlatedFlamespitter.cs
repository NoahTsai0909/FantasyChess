using UnityEngine;
using static CombatEventBus;

public class PlatedFlamespitter : UnitInstance
{
    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        base.EnterCombat(grid, row, col, isPlayer);

        if (isPassive)
        {
            CombatEventBus.OnCombatEvent += HandleCombatEvent;
        }
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= HandleCombatEvent;
    }

    protected override void HandleCombatEvent(CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        // Only care about ability use events
        if (type != CombatEventType.AbilityUsed) return;
        // Must be ally
        if (source.isPlayer != this.isPlayer) return;

        // Must be exactly one tile behind
        int expectedCol = isPlayer ? col - 1 : col + 1;
        if (source.row != row || source.col != expectedCol) return;

        // Now fire!
        UnitInstance enemy = FindNearestEnemy();
        if (enemy != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.ApplyBurn,
                    source = this,
                    target = enemy,
                    amount = stats.Burn,
                    reason = "Plated Flamespitter Passive",
                    isPassive = true
                }
            );
        }

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                target = this,
                amount = stats.Shield,
                reason = "Plated Flamespitter Passive",
                isPassive = true
            }
        );
    }

    public override string GetAbilityDescription()
    {
        return ($"Passive: When the ally behind this unit uses an ability, this unit burns the nearest enemy for {stats.Burn} and shields self for {stats.Shield}.");
    }
}
