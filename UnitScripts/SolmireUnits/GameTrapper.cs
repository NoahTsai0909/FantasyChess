using UnityEngine;

public class GameTrapper : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        if (target == null) return;
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Game Trapper Attack",
                isCrit = abilityCrit
            }
        );

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplySlow,
                source = this,
                target = target,
                amount = stats.Slow,
                reason = "Game Trapper Slow",
                isCrit = abilityCrit
            }
        );
    }

    public override string GetActiveDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage and slow it for {stats.Slow} seconds.");
    }
}
