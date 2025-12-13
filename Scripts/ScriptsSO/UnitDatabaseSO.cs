using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "Database/Unit Database")]
public class UnitDatabase : ScriptableObject
{
    public List<UnitDefinition> allUnits = new List<UnitDefinition>();

    // Static instance for easy access
    private static UnitDatabase _instance;
    public static UnitDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<UnitDatabase>("UnitDatabase");
                if (_instance == null)
                {
                    Debug.LogError("UnitDatabase not found in Resources folder!");
                }
            }
            return _instance;
        }
    }

    // Get units by tag (you'll need to add tags to UnitDefinition)
    public List<UnitDefinition> GetUnitsWithTags(UnitTagFlags requiredTags)
    {
        return allUnits.Where(unit =>
            (unit.tagFlags & requiredTags) == requiredTags).ToList();
    }

    // Get units by region
    public List<UnitDefinition> GetUnitsByRegion(string region)
    {
        return allUnits.Where(unit => unit.region == region).ToList();
    }

    // Get random unit
    public UnitDefinition GetRandomUnit()
    {
        if (allUnits.Count == 0) return null;
        return allUnits[Random.Range(0, allUnits.Count)];
    }

    // Get random unit by region
    public UnitDefinition GetRandomUnitByRegion(string region)
    {
        var regionalUnits = GetUnitsByRegion(region);
        if (regionalUnits.Count == 0) return GetRandomUnit();
        return regionalUnits[Random.Range(0, regionalUnits.Count)];
    }

    // We'll add rarity/tag filtering later
}

// Update UnitDefinition to include tags:
/*
public class UnitDefinition : ScriptableObject
{
    // ... existing fields ...
    
    [Header("Tags")]
    public List<string> tags = new List<string>();
    
    // ... rest of class ...
}
*/
