using UnityEngine;

public class ProdigiousMage : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindFarthestEnemy();
        if (target == null) return;
        if (currentEnergy <= 0) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Prodigious Mage Attack",
                isCrit = abilityCrit
            }
        );
    }
    public override string GetActiveDescription()
    {
        return ($"Attack the farthest enemy for {stats.Attack} damage.");
    }
}
