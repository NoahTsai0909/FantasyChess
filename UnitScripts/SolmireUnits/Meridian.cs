using UnityEngine;

public class Meridian : UnitInstance
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

        if (action.source.isPlayer != this.isPlayer) return;
        if (action.isCrit != true) return;
        this.TemporaryStatModify(ModifiableStats.Multicast, 1);
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindFarthestEnemy();

        if (target == null)
        {
            return;
        }

        CombatManager.Instance.ExecuteAction(
        new CombatAction
        {
            type = CombatActionType.Damage,
            source = this,
            target = target,
            amount = stats.Attack,
            reason = "Meridian Attack",
            isCrit = abilityCrit
        }
        );
    }

    public override string GetAbilityDescription()
    {
        return ($"Attack the farthest enemy for {stats.Attack} damage. Passive: When an ally crits, this gains 1 multicast.");
    }


}
