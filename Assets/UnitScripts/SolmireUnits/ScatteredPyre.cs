using UnityEngine;

public class ScatteredPyre : UnitInstance
{
    private bool hasUsedAbility = false;
    protected override void UseAbility()
    {
        base.UseAbility();
        hasUsedAbility = true;
        CombatManager.Instance.ExecuteAction(new CombatAction
        {
            type = CombatActionType.Kill,
            source = this,
            target = this,
            amount = 0,
            reason = "Scattered Pyre Self Destruct"
        });
    }

    public override string GetActiveDescription()
    {
        return ($"This dies. Summon a Cinder Resurgent.");
    }

    protected override void OnDeathEffect()
    {
        if (Definition.spawnDefinition != null && hasUsedAbility)
        {
            UnitSpawner.Instance.SpawnUnit(Definition.spawnDefinition, row, col, isPlayer, this, CurrentRarity);
            Debug.Log($"Scattered Pyre has died and summoned a Cinder Resurgent at ({row},{col})");
        }
    }
}
