using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Gain Random Unit")]
public class GainRandomUnitOutcomeSO : EventOutcomeSO
{
    public Region fallbackRegion;

    // NEW: Add a fallback tag 
    public UnitTagFlags fallbackTags = UnitTagFlags.None;

    public override void ExecuteOutcome(EventContext context)
    {
        if (context != null && context.generatedUnit != null)
        {
            PlayerUnitManager.Instance.TryAcquireUnit(context.generatedUnit.definition, context.generatedUnit.rarity);
        }
        else
        {
            UnitSaveData unit = UnitGenerationService.GenerateUnit(fallbackRegion, fallbackTags);
            PlayerUnitManager.Instance.TryAcquireUnit(unit.definition, unit.rarity);
        }
    }
}
