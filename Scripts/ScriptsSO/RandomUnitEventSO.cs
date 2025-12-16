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
        // Get random unit based on reputation and settings
        UnitDefinition randomUnit = GetRandomUnitByReputation();

        if (randomUnit != null)
        {
            // Add to bench
            RunManager.Instance.AddUnitToBench(randomUnit);
            Debug.Log($"Event Reward: Gained {randomUnit.unitName} (Rarity: {randomUnit.rarity})");
        }
        else
        {
            Debug.LogWarning("No suitable unit found for reward!");
        }
    }

    public override UnitDefinition ReturnRandomUnit()
    {
        UnitDefinition randomUnit = GetRandomUnitByReputation();
        return randomUnit;
    }

    private UnitDefinition GetRandomUnitByReputation()
    {
        // Simple implementation for now - just get random unit
        // Later: Filter by reputation, rarity weights, etc.
        if (UnitDatabase.Instance == null || UnitDatabase.Instance.allUnits.Count == 0)
            return null;

        // For now, get truly random unit from database
        return UnitDatabase.Instance.GetRandomUnit();
    }
}
