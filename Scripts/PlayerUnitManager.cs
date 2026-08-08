using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{

    public static PlayerUnitManager Instance { get; private set; }

    [Header("Grid Settings (For Overflow)")]
    [Tooltip("Set these to match your actual battle grid size!")]
    public int battleGridRows = 3;
    public int battleGridCols = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool TryAcquireUnit(UnitDefinition incomingDef, Rarity incomingRarity)
    {
        // 1. Try to Merge
        RunManager.UnitPlacement mergeTarget = FindMergeTarget(incomingDef, incomingRarity);
        if (mergeTarget != null)
        {
            MergeInto(mergeTarget);
            return true;
        }

        // 2. Try the Bench
        if (TryAddToBench(incomingDef, incomingRarity))
        {
            return true;
        }

        // 3. Bench is full! Overflow to the Battle Grid
        return TryAddToBattleGrid(incomingDef, incomingRarity);
    }

    RunManager.UnitPlacement FindMergeTarget(UnitDefinition def, Rarity rarity)
    {
        // Bench first (doesn't really matter, but feels intuitive)
        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            if (CanMerge(placement, def, rarity))
                return placement;
        }

        // Then grid
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (CanMerge(placement, def, rarity))
                return placement;
        }

        return null;
    }

    bool CanMerge(RunManager.UnitPlacement placement, UnitDefinition def, Rarity rarity)
    {
        if (placement.unitData == null)
            return false;

        if (placement.unitData.definition != def)
            return false;

        if (placement.unitData.rarity != rarity)
            return false;

        if (placement.unitData.rarity >= Rarity.Epic)
            return false;

        return true;
    }


    void MergeInto(RunManager.UnitPlacement placement)
    {
        placement.unitData.rarity += 1;

        Debug.Log(
            $"Merged into {placement.unitData.definition.unitName}, new tier {placement.unitData.rarity}"
        );
    }


    bool TryAddToBench(UnitDefinition def, Rarity rarity)
    {
        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            if (placement.unitData == null || placement.unitData.definition == null)
            {
                placement.unitData = new UnitSaveData
                {
                    definition = def,
                    rarity = rarity
                };
                return true;
            }
        }
        Debug.LogWarning("Bench full — attempting overflow...");
        return false;
    }

    bool TryAddToBattleGrid(UnitDefinition def, Rarity rarity)
    {
        // Iterate through all possible grid coordinates
        for (int r = 0; r < battleGridRows; r++)
        {
            for (int c = 0; c < battleGridCols; c++)
            {
                // Check if this coordinate is already occupied in the dynamic list
                bool isOccupied = RunManager.Instance.playerTeamPlacements.Exists(p => p.row == r && p.col == c);

                if (!isOccupied)
                {
                    // Create a new placement for the battle grid
                    RunManager.UnitPlacement newPlacement = new RunManager.UnitPlacement
                    {
                        row = r,
                        col = c,
                        unitData = new UnitSaveData
                        {
                            definition = def,
                            rarity = rarity
                        }
                    };

                    RunManager.Instance.playerTeamPlacements.Add(newPlacement);
                    Debug.Log($"Overflow Success! Placed {def.unitName} on the Battle Grid at ({r}, {c}).");
                    return true;
                }
            }
        }
        UniversalPopupManager.ShowPopup("Board Full! Incoming unit was discarded.");
        return false;
    }
}


