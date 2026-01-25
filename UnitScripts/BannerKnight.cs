using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BannerKnight : UnitInstance
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
                reason = "Banner Knight Attack"
            }
        );

        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage.");
    }

}
