using UnityEngine;

[CreateAssetMenu(fileName = "RandomUnitEvent", menuName = "Events/Random Unit Event")]
public class RandomUnitEventSO : BaseEventSO
{
    [Header("Unit Reward Settings")]
    public Rarity minRarity = Rarity.Common;
    public Rarity maxRarity = Rarity.Common;
    public Region region;
    public UnitTagFlags preferredTags = UnitTagFlags.None;

    private void ApplyRandomReward()
    {
        return;
    }

    public override UnitSaveData ReturnRandomUnit()
    {
        UnitSaveData randomUnit = UnitGenerationService.GenerateUnit(region, preferredTags);
        return randomUnit;
    }

    
}
