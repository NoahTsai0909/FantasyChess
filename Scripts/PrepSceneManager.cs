using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;

public class PrepSceneManager : MonoBehaviour
{
    public GridManager battleGrid;
    public GridManager benchGrid;
    [SerializeField] private Button ReturnButton;
    [SerializeField] private ProvisionManager provisionManager;
    [Header("Tactics")]
    [SerializeField] private TacticBarManager playerTacticBarManager;

    private List<UnitInstance> spawnedUnits = new List<UnitInstance>();

    void Start()
    {
        if (RunHUDManager.Instance != null)
        {
            RunHUDManager.Instance.SlideOutAndHide(0.5f);
        }

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
        LoadTacticBarFromRunManager();
        playerTacticBarManager.RefreshAllTacticAuras();
    }

    public void ReturnToMapScene()
    {
        // Set the encounter (you'll need to assign this somehow)
        // RunManager.Instance.currentEncounter = someEncounter;
        if (!provisionManager.IsProvisionValid())
        {
            Debug.LogWarning("Provision cap exceeded! Cannot leave prep scene.");
            // Optionally: Show a warning popup
            return;
        }
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

            // Place unit with isPlayer = true
            battleGrid.PlaceUnit(placement, placement.row, placement.col, null, true);

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
            if (placement.unitData != null && placement.unitData.definition != null && placement.unitData.definition.unitPrefab != null)
            {
                // Place bench unit with isPlayer = true
                benchGrid.PlaceUnit(placement, 0, col, null, true);

                UnitInstance spawned = benchGrid.GetUnitAtPosition(0, col);
                if (spawned == null)
                {
                    Debug.LogError($"Failed to get UnitInstance at bench column {col}");
                }
                else
                {
                    spawned.myPlacement = placement;
                    spawnedUnits.Add(spawned);
                }
            }

            col++;
        }
    }
    private void LoadTacticBarFromRunManager()
    {
        if (RunManager.Instance == null || playerTacticBarManager == null) return;

        playerTacticBarManager.ClearAllTactics();
        playerTacticBarManager.isCombatRunning = false; // Strictly enforce non-combat state

        // Ensure tactics are sorted by their saved orderIndex
        var sortedTactics = RunManager.Instance.playerTactics;
        sortedTactics.Sort((a, b) => a.orderIndex.CompareTo(b.orderIndex));

        foreach (var placement in sortedTactics)
        {
            if (placement.tacticData == null || placement.tacticData.definition == null) continue;

            TacticInstance tactic = Instantiate(placement.tacticData.definition.tacticPrefab);
            tactic.InitializeFromSaveData(placement.tacticData);
            tactic.myPlacement = placement;

            playerTacticBarManager.AddTactic(tactic);
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

        if (benchGrid != null)
        {
            for (int i = 0; i < RunManager.Instance.playerBenchPlacements.Count; i++)
            {
                // Query the specific slot on the physical grid (Row 0, Column i)
                UnitInstance unitInSlot = benchGrid.GetUnitAtPosition(0, i);

                if (unitInSlot != null && unitInSlot.myPlacement != null)
                {
                    // Unit exists here! Save its data.
                    RunManager.Instance.playerBenchPlacements[i].unitData = unitInSlot.myPlacement.unitData;
                }
                else
                {
                    // Slot is empty. Save as null.
                    RunManager.Instance.playerBenchPlacements[i].unitData = null;
                }

                // Hardcode bench coordinates
                RunManager.Instance.playerBenchPlacements[i].row = -1;
                RunManager.Instance.playerBenchPlacements[i].col = -1;
            }
        }
        if (playerTacticBarManager != null)
        {
            RunManager.Instance.playerTactics.Clear();
            var activeTactics = playerTacticBarManager.GetAllTactics();

            for (int i = 0; i < activeTactics.Count; i++)
            {
                if (activeTactics[i].myPlacement != null)
                {
                    activeTactics[i].myPlacement.orderIndex = i;
                    RunManager.Instance.playerTactics.Add(activeTactics[i].myPlacement);
                }
            }
            Debug.Log($"Saved {activeTactics.Count} tactics to RunManager");
        }

        Debug.Log("Saved battle grid and bench units to RunManager");
    }

}
