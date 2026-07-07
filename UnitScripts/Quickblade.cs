using UnityEngine;

public class Quickblade : UnitInstance 
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
                reason = "Quickblade Attack"
            }
        );
        base.UseAbility();
    }
    public override string GetAbilityDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage.");
    }
}
