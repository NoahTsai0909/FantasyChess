using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ClumsyKnight : UnitInstance
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
                reason = "Clumsy Knight Attack",
                isCrit = abilityCrit
            }
        );

    }

    public override string GetActiveDescription()
    {
        return ($"Attack the nearest enemy for [ATK] {stats.Attack}.");
    }

}
