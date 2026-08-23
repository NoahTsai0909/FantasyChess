using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ArmyCook : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        UnitInstance target2 = FindLowestHealthAlly();
        if (target == null && target2 == null) return;

        // Execute attack if there is an enemy
        if (target != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = stats.Attack,
                    reason = "Army Cook Attack",
                    isCrit = abilityCrit
                }
            );
        }

        // Execute heal if there is an ally
        if (target2 != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Heal,
                    source = this,
                    target = target2,
                    amount = stats.Heal,
                    reason = "Army Cook Heal",
                    isCrit = abilityCrit
                }
            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}. [c_heal]Heal[/c] the lowest health ally for [HEAL] {stats.Heal}.");
    }
}
