using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{

    public static PlayerUnitManager Instance { get; private set; }

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
        RunManager.UnitPlacement mergeTarget = FindMergeTarget(incomingDef, incomingRarity);

        if (mergeTarget != null)
        {
            MergeInto(mergeTarget);
            return true;
        }

        return TryAddToBench(incomingDef, incomingRarity);
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
        Debug.Log($"Bench slot count = {RunManager.Instance.playerBenchPlacements.Count}");

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

        Debug.LogWarning("Bench full — unit discarded");
        return false;
    }

}

