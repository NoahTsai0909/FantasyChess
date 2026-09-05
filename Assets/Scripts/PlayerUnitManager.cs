using UnityEngine;
public enum TransformRule { Same, Any, Higher, Lower, Different }

[System.Serializable]
public struct TransformParams
{
    public TransformRule rarityRule;
    public TransformRule regionRule;
    public TransformRule provisionRule;
    public bool keepMutations;
}

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

    public bool TryAcquireUnit(UnitDefinition incomingDef, Rarity incomingRarity, MutationPrefixSO incomingPrefix = null, MutationSuffixSO incomingSuffix = null)
    {
        RunManager.UnitPlacement mergeTarget = FindMergeTarget(incomingDef, incomingRarity);
        if (mergeTarget != null)
        {
            MergeInto(mergeTarget, incomingPrefix, incomingSuffix);
            return true;
        }

        if (TryAddToBench(incomingDef, incomingRarity, incomingPrefix, incomingSuffix))
        {
            return true;
        }

        return TryAddToBattleGrid(incomingDef, incomingRarity, incomingPrefix, incomingSuffix);
    }

    RunManager.UnitPlacement FindMergeTarget(UnitDefinition def, Rarity rarity)
    {
        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            if (CanMerge(placement, def, rarity)) return placement;
        }

        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (CanMerge(placement, def, rarity)) return placement;
        }

        return null;
    }

    bool CanMerge(RunManager.UnitPlacement placement, UnitDefinition def, Rarity rarity)
    {
        if (placement.unitData == null) return false;
        if (placement.unitData.definition != def) return false;
        if (placement.unitData.rarity != rarity) return false;
        if (placement.unitData.rarity >= Rarity.Epic) return false;

        return true;
    }

    void MergeInto(RunManager.UnitPlacement placement, MutationPrefixSO incomingPrefix, MutationSuffixSO incomingSuffix)
    {
        placement.unitData.rarity += 1;

        // If the purchased unit has a mutation, overwrite
        // If the purchased unit has NO mutation, it bypasses this and keeps the original
        if (incomingPrefix != null)
        {
            placement.unitData.prefix = incomingPrefix;
            placement.unitData.suffix = incomingSuffix;
        }

        Debug.Log(
            $"Merged into {placement.unitData.definition.unitName}, new tier {placement.unitData.rarity}"
        );
    }


    bool TryAddToBench(UnitDefinition def, Rarity rarity, MutationPrefixSO prefix, MutationSuffixSO suffix)
    {
        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            if (placement.unitData == null || placement.unitData.definition == null)
            {
                placement.unitData = new UnitSaveData
                {
                    definition = def,
                    rarity = rarity,
                    prefix = prefix, 
                    suffix = suffix  
                };
                return true;
            }
        }
        Debug.LogWarning("Bench full — attempting overflow...");
        return false;
    }

    bool TryAddToBattleGrid(UnitDefinition def, Rarity rarity, MutationPrefixSO prefix, MutationSuffixSO suffix)
    {
        for (int r = 0; r < battleGridRows; r++)
        {
            for (int c = 0; c < battleGridCols; c++)
            {
                bool isOccupied = RunManager.Instance.playerTeamPlacements.Exists(p => p.row == r && p.col == c);

                if (!isOccupied)
                {
                    RunManager.UnitPlacement newPlacement = new RunManager.UnitPlacement
                    {
                        row = r,
                        col = c,
                        unitData = new UnitSaveData
                        {
                            definition = def,
                            rarity = rarity,
                            prefix = prefix, 
                            suffix = suffix  
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

    public bool TransformUnit(UnitInstance targetUnit, TransformParams rules)
    {
        if (targetUnit == null || targetUnit.myPlacement == null) return false;

        UnitDefinition oldDef = targetUnit.Definition;
        Rarity oldRarity = targetUnit.CurrentRarity;

        Rarity targetRarity = oldRarity;
        if (rules.rarityRule == TransformRule.Higher && oldRarity < Rarity.Epic) targetRarity++;
        else if (rules.rarityRule == TransformRule.Lower && oldRarity > Rarity.Common) targetRarity--;
        else if (rules.rarityRule == TransformRule.Any) targetRarity = RunManager.Instance.RollRarityForDay(RunManager.Instance.Stats.CurrentDay);

        int minProv = 0;
        int maxProv = -1; 
        if (rules.provisionRule == TransformRule.Same)
        {
            minProv = oldDef.provisionCost;
            maxProv = oldDef.provisionCost;
        }
        else if (rules.provisionRule == TransformRule.Higher)
        {
            minProv = oldDef.provisionCost + 1;
        }
        else if (rules.provisionRule == TransformRule.Lower)
        {
            maxProv = Mathf.Max(0, oldDef.provisionCost - 1);
        }

        Region? targetRegion = null;
        if (rules.regionRule == TransformRule.Same) targetRegion = oldDef.region;

        UnitDefinition newDef = UnitDatabase.Instance.GetRandomUnit(
            targetRarity, targetRegion, UnitTagFlags.None, minProv, maxProv
        );

        if (newDef == null) return false;

        RunManager.UnitPlacement placement = targetUnit.myPlacement;
        placement.unitData.definition = newDef;
        placement.unitData.rarity = targetRarity;

        if (!rules.keepMutations)
        {
            placement.unitData.prefix = null;
            placement.unitData.suffix = null;
        }

        GridManager grid = targetUnit.myGrid;
        int r = placement.row;
        int col = placement.col;
        bool isPlayer = targetUnit.isPlayer;

        grid.PlaceUnit(placement, r, col, null, isPlayer);

        return true;
    }
}


