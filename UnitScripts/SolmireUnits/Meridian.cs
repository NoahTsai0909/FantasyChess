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
        Debug.Log($"Meridian gained 1 multicast from ally crit! Current multicast: {stats.Multicast}");
    }

    protected override void UseAbility()
    {
        Debug.Log("A");

        base.UseAbility();

        Debug.Log("B");

        UnitInstance target = FindFarthestEnemy();

        Debug.Log("C");

        if (target == null)
        {
            Debug.Log("NULL");
            return;
        }

        Debug.Log("D");

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

        Debug.Log("E");
    }

    public override string GetAbilityDescription()
    {
        return ($"Attack the farthest enemy for {stats.Attack} damage. Passive: When an ally crits, this gains 1 multicast.");
    }


}
