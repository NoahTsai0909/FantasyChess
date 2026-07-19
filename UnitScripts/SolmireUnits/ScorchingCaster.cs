using UnityEngine;

public class ScorchingCaster : UnitInstance
{
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
                reason = "Scorching Caster Attack",
                isCrit = abilityCrit
            }
        );

        target = FindFarthestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyBurn,
                source = this,
                target = target,
                amount = stats.Burn,
                reason = "Scorching Caster Burn",
                isCrit = abilityCrit
            }
        );

    }
    public override string GetActiveDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage. Burn the farthest enemy for {stats.Burn}." );
    }
}
