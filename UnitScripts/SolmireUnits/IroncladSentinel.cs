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
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}. If this is [c_shield]shielded[/c], [c_attack]attack[/c] for [ATK] {2 * stats.Attack} instead.");
    }
}
