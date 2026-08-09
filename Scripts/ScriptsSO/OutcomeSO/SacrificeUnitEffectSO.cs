using UnityEngine;

[CreateAssetMenu(menuName = "Unit Effects/Sacrifice Unit")]
public class SacrificeUnitEffectSO : UnitTargetEffectSO
{
    public override void ApplyEffect(RunManager.UnitPlacement placement)
    {
        if (placement == null || placement.unitData == null) return;

        Debug.Log($"Sacrificing unit: {placement.unitData.definition.unitName}");

        // 1. If it's on the dynamic Battle Grid, remove it entirely
        if (RunManager.Instance.playerTeamPlacements.Contains(placement))
        {
            RunManager.Instance.playerTeamPlacements.Remove(placement);
        }
        // 2. If it's on the fixed-size Bench, just clear the data to leave an empty slot
        else if (RunManager.Instance.playerBenchPlacements.Contains(placement))
        {
            placement.unitData = null;
        }
    }
}
