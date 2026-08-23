using System;
using UnityEngine;

public static class RunStatsEventBus
{
    // Gold events
    public static event Action<int> OnGoldChanged;
    public static void GoldChanged(int newGold) => OnGoldChanged?.Invoke(newGold);

    // Health events
    public static event Action<int> OnHealthChanged;
    public static void HealthChanged(int newHealth) => OnHealthChanged?.Invoke(newHealth);

    // Day events
    public static event Action<int> OnDayChanged;
    public static void DayChanged(int newDay) => OnDayChanged?.Invoke(newDay);

    // Level events
    public static event Action<int> OnLevelChanged;
    public static void LevelChanged(int newLevel) => OnLevelChanged?.Invoke(newLevel);

    // Reputation/XP events
    public static event Action<int> OnReputationChanged;
    public static void ReputationChanged(int newReputation) => OnReputationChanged?.Invoke(newReputation);

    // Provision Cap events
    public static event Action<int> OnProvisionCapChanged;
    public static void ProvisionCapChanged(int newCap) => OnProvisionCapChanged?.Invoke(newCap);

}
