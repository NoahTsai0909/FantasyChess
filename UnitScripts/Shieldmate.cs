using UnityEngine;

public class Shieldmate : UnitInstance
{
    protected override void UseAbility()
    {

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = this,
                amount = stats.Shield,
                reason = "Shieldmate shielding"
            }
        );

        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        stats = RunManager.Instance.GetPreviewStats(Definition, CurrentRarity);
        return ($"Shields self for {stats.Shield}.");
    }
}
