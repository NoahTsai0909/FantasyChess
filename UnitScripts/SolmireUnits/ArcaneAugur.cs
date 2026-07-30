using UnityEngine;

public class ArcaneAugur : UnitInstance
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

        if (action.source.isPlayer != this.isPlayer || action.source.isEnergy == false || action.source != this) return;

        currentEnergy = Mathf.Min(currentEnergy + 1, stats.maxEnergy);
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        if (target == null) return;
        if (currentEnergy <= 0) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Arcane Augur Attack",
                isCrit = abilityCrit
            }
        );
    }
    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}.");
    }

    public override string GetPassiveDescription()
    {
        return ("Whenever another ally uses [ENERGY], recharge [c_energy]1[/c] [ENERGY].");
    }
}
