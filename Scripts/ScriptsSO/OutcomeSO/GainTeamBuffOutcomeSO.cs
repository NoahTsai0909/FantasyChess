using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Gain Team Buff")]
public class GainTeamBuffOutcomeSO : EventOutcomeSO
{

    // NEW: Add a fallback tag 
    public int buffAmount;
    public ModifiableStats buffType;

    public override void ExecuteOutcome(EventContext context)
    {
        Debug.Log($"Applying Team Buff! Granting +{buffAmount} to {buffType} for all owned units.");

        // 1. Buff everyone currently on the Battle Grid
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            ApplyBuffToPlacement(placement);
        }

        // 2. Buff everyone currently on the Bench
        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            ApplyBuffToPlacement(placement);
        }
    }

    private void ApplyBuffToPlacement(RunManager.UnitPlacement placement)
    {
        // Skip empty grid/bench slots
        if (placement == null || placement.unitData == null) return;

        // Retrieve the persistent stats for this specific unit's Guid
        PermanentStats pStats = RunManager.Instance.GetPermanentStatsForUnit(placement.unitData.id);

        if (pStats == null)
        {
            pStats = RunManager.Instance.CreatePermanentStatsForUnit(placement.unitData.id);
        }

        // Apply the correct stat bonus based on the enum
        switch (buffType)
        {
            case ModifiableStats.MaxHP:
                pStats.bonusMaxHP += buffAmount;
                break;
            case ModifiableStats.Attack:
                pStats.bonusAttack += buffAmount;
                break;
            case ModifiableStats.Heal:
                pStats.bonusHeal += buffAmount;
                break;
            case ModifiableStats.Shield:
                pStats.bonusShield += buffAmount;
                break;
            case ModifiableStats.Burn:
                pStats.bonusBurn += buffAmount;
                break;
            case ModifiableStats.Poison:
                pStats.bonusPoison += buffAmount;
                break;
            case ModifiableStats.MaxEnergy:
                pStats.bonusMaxEnergy += buffAmount;
                break;
            case ModifiableStats.CritChance:
                pStats.bonusCritChance += buffAmount;
                break;
            case ModifiableStats.Multicast:
                pStats.bonusMulticast += buffAmount;
                break;
            default:
                Debug.LogWarning($"Unhandled stat type {buffType} in GainTeamBuffOutcomeSO!");
                break;
        }
    }
}