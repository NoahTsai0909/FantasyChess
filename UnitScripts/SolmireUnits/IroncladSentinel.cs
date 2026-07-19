using UnityEngine;

public class IroncladSentinel : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        if (target == null) return;

        if (this.GetCurrentShield() > 0){
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = 2 * stats.Attack,
                reason = "Ironclad Sentinel Attack",
                isCrit = abilityCrit
            }
            );
        }
        else
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Ironclad Sentinel Attack",
                isCrit = abilityCrit
            }
            );
        }
    }
    public override string GetActiveDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage. If this is shielded, attack for {2 * stats.Attack} instead.");
    }
}
