using System.Collections.Generic;
using UnityEngine;

public class ProvisionManager : MonoBehaviour
{
    [SerializeField] private GridManager battleGrid;
    [SerializeField] private GridManager benchGrid;
    private RunManager runManager;

    [Header("UI")]
    [SerializeField] public TMPro.TextMeshProUGUI provisionText;
    [SerializeField] private Color validColor = Color.white;
    [SerializeField] public Color exceededColor = Color.red;

    private int currentProvisionUsed = 0;

    void Start()
    {
        if (runManager == null)
            runManager = RunManager.Instance;

        CalculateCurrentProvision();
        UpdateUI();
    }

    public int GetUnitProvisionCost(UnitInstance unit)
    {
        if (unit == null || unit.myPlacement == null || unit.myPlacement.unitData == null)
            return 0;

        // Use the provision cost from UnitSaveData if available,
        // otherwise fall back to the UnitDefinition default
        return unit.myPlacement.unitData.provisionCost > 0
            ? unit.myPlacement.unitData.provisionCost
            : unit.Definition.provisionCost;
    }

    public int CalculateProvisionForGrid(GridManager grid)
    {
        int total = 0;
        var units = grid.GetAllUnits();

        foreach (var unit in units)
        {
            total += GetUnitProvisionCost(unit);
        }

        return total;
    }

    public void CalculateCurrentProvision()
    {
        currentProvisionUsed = CalculateProvisionForGrid(battleGrid);
        UpdateUI();
    }

    public bool CanAddUnitToBattleGrid(UnitInstance unit)
    {
        int unitCost = GetUnitProvisionCost(unit);
        int projectedTotal = currentProvisionUsed + unitCost;

        return projectedTotal <= runManager.provisionCap;
    }

    public bool CanSwapUnits(UnitInstance unitLeavingBattle, UnitInstance unitEnteringBattle)
    {
        // This method assumes: unitLeavingBattle is currently in battle grid
        //                     unitEnteringBattle is trying to enter battle grid

        int leavingCost = GetUnitProvisionCost(unitLeavingBattle);
        int enteringCost = GetUnitProvisionCost(unitEnteringBattle);
        int netChange = enteringCost - leavingCost;

        return currentProvisionUsed + netChange <= runManager.provisionCap;
    }

    public bool IsProvisionValid()
    {
        return currentProvisionUsed <= runManager.provisionCap;
    }

    private void UpdateUI()
    {
        if (provisionText == null) return;

        provisionText.text = $"{currentProvisionUsed}/{runManager.provisionCap}";
        provisionText.color = IsProvisionValid() ? validColor : exceededColor;
    }

    // Call this whenever units are moved
    public void OnUnitMoved(GridManager fromGrid, GridManager toGrid, UnitInstance unit)
    {
        CalculateCurrentProvision();
    }
}
