using UnityEngine;

public class MendingCaster : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
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
                reason = "Mending Caster Heal",
                isCrit = abilityCrit
            }
        );
    }
    public override string GetActiveDescription()
    {
        return ($"[c_heal]Heal[/c] the lowest health ally for [HEAL] {stats.Heal}.");
    }
}
