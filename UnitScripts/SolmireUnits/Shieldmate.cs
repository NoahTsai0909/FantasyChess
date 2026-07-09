using UnityEngine;

public class Shieldmate : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = this,
                amount = stats.Shield,
                reason = "Shieldmate shielding",
                isCrit = abilityCrit
            }
        );

    }

    public override string GetAbilityDescription()
    {
        return ($"Shields self for {stats.Shield}.");
    }
}
