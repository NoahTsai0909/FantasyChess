using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Rarity Distribution Table")]
public class RarityDistributionTable : ScriptableObject
{
    public List<DayRarityEntry> days;

    public DayRarityEntry GetForDay(int day)
    {
        if (days.Count == 0)
            return null;

        // Clamp to last entry if day exceeds table
        for (int i = days.Count - 1; i >= 0; i--)
        {
            if (day >= days[i].day)
                return days[i];
        }

        return days[0];
    }

    public static Rarity RollRarity(DayRarityEntry dist)
    {
        int roll = Random.Range(0, 100);
        int cumulative = 0;

        cumulative += dist.common;
        if (roll < cumulative) return Rarity.Common;

        cumulative += dist.uncommon;
        if (roll < cumulative) return Rarity.Uncommon;

        cumulative += dist.rare;
        if (roll < cumulative) return Rarity.Rare;

        return Rarity.Epic;
    }
}

[System.Serializable]
public class DayRarityEntry
{
    public int day;
    [Range(0, 100)] public int common;
    [Range(0, 100)] public int uncommon;
    [Range(0, 100)] public int rare;
    [Range(0, 100)] public int epic;
}

