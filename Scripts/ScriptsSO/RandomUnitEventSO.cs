using UnityEngine;

[CreateAssetMenu(fileName = "RandomUnitEvent", menuName = "Events/Random Unit Event")]
public class RandomUnitEventSO : BaseEventSO
{
    [Header("Unit Reward Settings")]
    public Rarity minRarity = Rarity.Common;
    public Rarity maxRarity = Rarity.Common;
    public UnitTagFlags preferredTags = UnitTagFlags.None;

    private void ApplyRandomReward()
    {
        return;
    }

    public override UnitDefinition ReturnRandomUnit()
    {
        UnitDefinition randomUnit = GetRandomUnitByReputation();
        return randomUnit;
    }

    private UnitDefinition GetRandomUnitByReputation()
    {
        if (UnitDatabase.Instance == null || UnitDatabase.Instance.allUnits.Count == 0)
            return null;

        // For now, get truly random unit from database
        return UnitDatabase.Instance.GetRandomUnit();
    }
}
