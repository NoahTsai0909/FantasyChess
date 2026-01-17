using UnityEngine;

public class Hatchling : UnitInstance
{
    protected override void UseAbility()
    {

        UnitInstance target = FindNearestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyBurn,
                source = this,
                target = target,
                amount = stats.Burn,
                reason = "Hatchling Burn."
            }
        );

        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        stats = RunManager.Instance.GetPreviewStats(Definition, CurrentRarity);
        return ($"Burn the nearest enemy for {stats.Burn}.");
    }
}
