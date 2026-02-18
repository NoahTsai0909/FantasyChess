using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ArmyCook : UnitInstance
{
    protected override void UseAbility()
    {

        UnitInstance target = FindNearestEnemy();
        UnitInstance target2 = FindLowestHealthAlly();
        if (target == null || target2 == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Army Cook Attack"
            }
        );

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = target2,
                amount = stats.Heal,
                reason = "Army Cook Heal"
            }
        );

        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage. Heal the lowest health ally for {stats.Heal} health.");
    }
}
