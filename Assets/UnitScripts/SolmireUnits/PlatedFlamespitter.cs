using UnityEngine;
using static CombatEventBus;

public class PlatedFlamespitter : UnitInstance
{
    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);

        CombatEventBus.OnCombatEvent += HandleCombatEvent;
        
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

        if (inCombat && currentSuffix != null)
        {
            currentSuffix.ExecuteEffect(this);
        }

    }


    public override string GetPassiveDescription()
    {
        return ($"When the ally behind this unit uses an ability, [c_burn]burn[/c] the nearest enemy for [BURN] {stats.Burn} and [c_shield]shield[/c] this for [SHIELD] {stats.Shield}.");
    }
    public override string GetMutationTriggerText()
    {
        return ($"<br>Also ");
    }
}
