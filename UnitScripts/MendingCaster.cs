using UnityEngine;

public class MendingCaster : UnitInstance
{
    protected override void UseAbility()
    {

        UnitInstance target = FindLowestHealthAlly();
        if (target == null) return;
        if (currentEnergy <= 0) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = target,
                amount = stats.Heal,
                reason = "Mending Caster Heal"
            }
        );

        target = FindLowestHealthAlly();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = target,
                amount = stats.Heal,
                reason = "Mending Caster Heal"
            }
        );

        base.UseAbility();
    }
    public override string GetAbilityDescription()
    {
        return ($"Heal the lowest health ally for {stats.Heal}.");
    }
}
