using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;

public class PrepSceneManager : MonoBehaviour
{
    [SerializeField] private GridManager battleGrid; // 3x3
    [SerializeField] private GridManager benchGrid;  // 1x8
    [SerializeField] private Button ReturnButton;

    private List<UnitInstance> spawnedUnits = new List<UnitInstance>();

    void Start()
    {

        if (RunManager.Instance != null)
        {
            RunManager.Instance.SanitizeBench();
        }

        ReturnButton.onClick.AddListener(() => {
            ReturnToMapScene();
        });
        DragAndDropManager dragManager = FindFirstObjectByType<DragAndDropManager>();

        LoadBattleGridFromRunManager();

        LoadBenchGridFromRunManager();
    }

    public void ReturnToMapScene()
    {
        // Set the encounter (you'll need to assign this somehow)
        // RunManager.Instance.currentEncounter = someEncounter;
        SaveCurrentTeamToRunManager();
        SceneLoader.Instance.LoadScene(SceneLoader.Instance.lastScene);
    }

    private void LoadBattleGridFromRunManager()
    {
        if (RunManager.Instance == null) return;

        TeamDefinition playerTeam = RunManager.Instance.GetTeamForCombat();
        if (playerTeam == null) return;

        foreach (var placement in playerTeam.units)
        {
            if (placement.unitData == null)
            {
                Debug.LogWarning("Skipping placement with null UnitSaveData");
                continue;
            }

            if (placement.unitData.definition == null || placement.unitData.definition.unitPrefab == null)
            {
                Debug.LogWarning($"Skipping placement: missing prefab or definition for {placement.unitData.definition?.name ?? "NULL"}");
                continue;
            }

            // Place unit, GridManager spawns the visual
            battleGrid.PlaceUnit(placement, placement.row, placement.col);

            // Reference to runtime UnitInstance
            UnitInstance spawned = battleGrid.GetUnitAtPosition(placement.row, placement.col);
            if (spawned == null)
            {
                Debug.LogError($"Failed to get UnitInstance at {placement.row},{placement.col} after PlaceUnit");
                continue;
            }

            spawned.isPlayer = true;
            spawned.myPlacement = placement;

            spawnedUnits.Add(spawned);
        }
    }



    private void LoadBenchGridFromRunManager()
    {
        if (RunManager.Instance == null) return;

        TeamDefinition benchTeam = RunManager.Instance.GetTeamForBench();
        if (benchTeam == null || benchTeam.units == null) return;

        int col = 0;
        foreach (var placement in benchTeam.units)
        {
            if (placement.unitData == null || placement.unitData.definition == null || placement.unitData.definition.unitPrefab == null)
            {
                Debug.LogWarning($"Skipping bench placement with missing prefab/definition");
                continue;
            }

            benchGrid.PlaceUnit(placement, 0, col);

            UnitInstance spawned = benchGrid.GetUnitAtPosition(0, col);
            if (spawned == null)
            {
                Debug.LogError($"Failed to get UnitInstance at bench column {col}");
                continue;
            }

            spawned.myPlacement = placement;
            spawnedUnits.Add(spawned);
            col++;
        }
    }

    private void SaveCurrentTeamToRunManager()
    {
        if (RunManager.Instance == null) return;

        // ------------------ Save Battle Grid Units ------------------
        List<RunManager.UnitPlacement> battleTeam = new List<RunManager.UnitPlacement>();

        foreach (UnitInstance unit in battleGrid.GetAllUnits())
        {
            if (unit.myPlacement == null)
            {
                Debug.LogWarning($"Unit {unit.name} has null myPlacement!");
                continue;
            }

            // Update placement row/col
            unit.myPlacement.row = battleGrid.GetUnitPosition(unit).x;
            unit.myPlacement.col = battleGrid.GetUnitPosition(unit).y;

            battleTeam.Add(unit.myPlacement);

            Debug.Log($"Saving {unit.Definition.name} at row={unit.myPlacement.row}, col={unit.myPlacement.col}");
        }

        RunManager.Instance.playerTeamPlacements = battleTeam;

        // ------------------ Save Bench Units ------------------
        List<UnitInstance> benchUnits = benchGrid.GetAllUnits();

        for (int i = 0; i < RunManager.Instance.playerBenchPlacements.Count; i++)
        {
            if (i < benchUnits.Count)
            {
                UnitInstance unit = benchUnits[i];
                RunManager.Instance.playerBenchPlacements[i].unitData = unit.myPlacement.unitData;

                // Indicate bench by row/col = -1
                RunManager.Instance.playerBenchPlacements[i].row = -1;
                RunManager.Instance.playerBenchPlacements[i].col = -1;
            }
            else
            {
                // Empty slot
                RunManager.Instance.playerBenchPlacements[i].unitData = null;
                RunManager.Instance.playerBenchPlacements[i].row = -1;
                RunManager.Instance.playerBenchPlacements[i].col = -1;
            }
        }

        Debug.Log("Saved battle grid and bench units to RunManager");
    }


    void ClearGrid(GridManager grid)
    {
        Debug.Log($"Clearing grid: {grid.name}");

        // Use the new ClearAllUnits method
        grid.ClearAllUnits();

        // Find all UnitInstance objects in the scene
        // Use FindObjectsByType with FindObjectsSortMode.None
        UnitInstance[] allUnitsInScene = FindObjectsByType<UnitInstance>(FindObjectsSortMode.None);

        foreach (UnitInstance unit in allUnitsInScene)
        {
            if (unit != null && !spawnedUnits.Contains(unit))
            {
                Debug.Log($"Destroying stray unit: {unit.name}");
                Destroy(unit.gameObject);
            }
        }
    }

}
