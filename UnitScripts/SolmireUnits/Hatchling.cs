using UnityEngine;

public class Hatchling : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyBurn,
                source = this,
                target = target,
                amount = stats.Burn,
                reason = "Hatchling Burn.",
                isCrit = abilityCrit
            }
        );

        Debug.Log($"Hatchling burn is {stats.Burn}");

    }

    public override string GetActiveDescription()
    {
        /*stats = RunManager.Instance.GetPreviewStats(Definition, CurrentRarity);*/
        return ($"Burn the nearest enemy for {stats.Burn}.");
    }
}
