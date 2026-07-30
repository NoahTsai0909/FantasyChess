using UnityEngine;

public class Kindling : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyBurn,
                source = this,
                target = target,
                amount = stats.Burn,
                reason = "Kindling Burn",
                isCrit = abilityCrit
            }
        );

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = this,
                amount = stats.Heal,
                reason = "Kindling Heal",
                isCrit = abilityCrit
            }
        );
    }

    public override string GetActiveDescription()
    {
        return ($"[c_burn]Burn[/c] the nearest enemy for [BURN] {stats.Burn}. [c_heal]Heal[/c] this for [HEAL] {stats.Heal}.");
    }
}
