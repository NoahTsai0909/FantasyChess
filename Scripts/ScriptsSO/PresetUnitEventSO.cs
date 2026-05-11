using UnityEngine;

[CreateAssetMenu(fileName = "PresetUnitEvent", menuName = "Events/Preset Unit Event")]
public class PresetUnitEventSO : BaseEventSO
{
    [Header("Unit Reward Settings")]
    public UnitDefinition presetUnit;
    public bool isUnit;

    public override UnitSaveData ReturnRandomUnit()
    {
        int day = RunManager.Instance.Stats.CurrentDay;

        // Get rarity distribution for this day
        DayRarityEntry dist = RunManager.Instance.rarityDistributionTable.GetForDay(day);

        // Roll rarity
        Rarity rolledRarity = RarityDistributionTable.RollRarity(dist);

        return new UnitSaveData
        {
            definition = presetUnit,
            rarity = rolledRarity
        };
    }

}
