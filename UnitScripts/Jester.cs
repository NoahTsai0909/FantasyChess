using UnityEngine;

public class Jester : UnitInstance
{
    protected override void UseAbility()
    {

        UnitInstance target = FindRandomEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Jester Attack"
            }
        );
        base.UseAbility();
    }
    public override string GetAbilityDescription()
    {
        return ($"Attack a random enemy for {stats.Attack} damage.");
    }
}
