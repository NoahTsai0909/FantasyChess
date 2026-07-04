using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SearingHalo : UnitInstance
{
    List<UnitInstance> enemies = new List<UnitInstance>();

    private int advanceCount = 1;
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

        if (action.type != CombatActionType.Heal || action.target != this) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Advance,
                source = this,
                target = this,
                amount = advanceCount,
                reason = "Searing Halo Passive"
            }
        );

        Debug.Log($"Searing Halo passive triggered: Advanced {advanceCount} second");
    }

    protected override void UseAbility()
    {

        enemies = FindAllEnemies();
        foreach (UnitInstance enemy in enemies)
        {
            if (enemy != null && enemy != this)
            {
                CombatManager.Instance.ExecuteAction(
                    new CombatAction
                    {
                        type = CombatActionType.ApplyBurn,
                        source = this,
                        target = enemy,
                        amount = stats.Burn,
                        reason = "Searing Halo Burn"
                    }
                );
            }
        }

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = this,
                amount = stats.Heal,
                reason = "Searing Halo Heal"
            }
        );

        base.UseAbility();
    }
    public override string GetAbilityDescription()
    {
        return ($"Burn all enemies for {stats.Burn}. Heal this for {stats.Heal}. Whenever this is healed, advance this 1 second.");
    }
}
