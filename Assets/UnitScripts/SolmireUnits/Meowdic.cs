using System.Collections.Generic;
using UnityEngine;

public class Meowdic : UnitInstance
{
    private int advanceCount = 1;
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
        if (action.type != CombatActionType.Heal || action.source.isPlayer != this.isPlayer) return;
       
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Advance,
                source = this,
                target = this,
                amount = advanceCount,
                reason = "Meowdic Passive"
            }
        );

        Debug.Log($"Meowdic passive triggered: Advanced {advanceCount} second(s) due to healing an ally.");
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> targets = FindSideAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = target,
                amount = stats.Heal,
                reason = "Meowdic Heal",
                isCrit = abilityCrit
            }

            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_heal]Heals[/c] [c_side]side[/c] allies for [HEAL] {stats.Heal}.");
    }

    public override string GetPassiveDescription()
    {
        return ("When an ally is [c_heal]healed[/c], advance this 1 second.");
    }
}
