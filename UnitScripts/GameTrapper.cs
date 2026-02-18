using UnityEngine;

public class GameTrapper : UnitInstance
{
    protected override void UseAbility()
    {

        UnitInstance target = FindNearestEnemy();
        if (target == null) return;
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Game Trapper Attack"
            }
        );

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplySlow,
                source = this,
                target = target,
                amount = stats.Slow,
                reason = "Game Trapper Slow"
            }
        );

        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage and slow it for {stats.Slow} seconds.");
    }
}
