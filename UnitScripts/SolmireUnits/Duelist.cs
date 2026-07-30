using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Duelist : UnitInstance
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
                reason = "Duelist Attack",
                isCrit = abilityCrit
            }
        );

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = this,
                amount = stats.Shield,
                reason = "Duelist self shield",
                isCrit = abilityCrit
            }
        );
    }
    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack} damage. [c_shield]Shield[/c] this for [SHIELD] {stats.Shield}.");
    }
}
