using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;

public class PrepSceneManager : MonoBehaviour
{
    [SerializeField] private GridManager battleGrid; // 3x3
    [SerializeField] private GridManager benchGrid;  // 1x8
    [SerializeField] private Button ReturnButton;

    public class UnitPlacement
    {
        public UnitInstance unitPrefab;
        public int row;
        public int col;
    }

    private List<UnitInstance> spawnedUnits = new List<UnitInstance>();

    void Start()
    {
        ReturnButton.onClick.AddListener(() => {
            ReturnToMapScene();
        });
        DragAndDropManager dragManager = FindFirstObjectByType<DragAndDropManager>();

        ClearAllUnits();

        LoadBattleGridFromRunManager();

        LoadBenchGridFromRunManager();

        AddCollidersToUnits();
    }

    public void ReturnToMapScene()
    {
        // Set the encounter (you'll need to assign this somehow)
        // RunManager.Instance.currentEncounter = someEncounter;
        SaveCurrentTeamToRunManager();
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }

    private void LoadBattleGridFromRunManager()
    {
        if (RunManager.Instance != null)
        {
            TeamDefinition playerTeam = RunManager.Instance.GetTeamForCombat();
            if (playerTeam != null)
            {
                foreach (var unitPlacement in playerTeam.units)
                {
                    if (unitPlacement.unitPrefab != null)
                    {
                        UnitInstance unit = SpawnUnit(unitPlacement.unitPrefab, battleGrid,
                                 unitPlacement.row, unitPlacement.col, true);
                        spawnedUnits.Add(unit);
                    }
                }
            }
        }
    }

    private void LoadBenchGridFromRunManager()
    {
        if (RunManager.Instance != null)
        {
            TeamDefinition playerBench = RunManager.Instance.GetTeamForBench();
            if (playerBench != null)
            {
                foreach (var unitPlacement in playerBench.units)
                {
                    if (unitPlacement.unitPrefab != null)
                    {
                        UnitInstance unit = SpawnUnit(unitPlacement.unitPrefab, benchGrid,
                                 unitPlacement.row, unitPlacement.col, true);
                        spawnedUnits.Add(unit);
                    }
                }
            }
        }
    }

    private UnitInstance SpawnUnit(UnitInstance unitPrefab, GridManager grid,
                                   int row, int col, bool isPlayer)
    {
        // 1. Instantiate the SPECIFIC prefab (BannerKnight, SolemnPriest, etc.)
        UnitInstance unit = Instantiate(unitPrefab);

        unit.SetSourcePrefab(unitPrefab);
        // 2. Set team
        unit.isPlayer = isPlayer;

        // 3. Initialize with UI prefabs
        grid.PlaceUnit(unit, row, col);

        return unit;
    }

    private void SaveCurrentTeamToRunManager()
    {
        if (RunManager.Instance != null)
        {
            // Save battle team
            List<RunManager.UnitPlacement> battleTeam = new List<RunManager.UnitPlacement>();
            foreach (UnitInstance unit in battleGrid.GetAllUnits())
            {
                Vector2Int pos = battleGrid.GetUnitPosition(unit);
                if (pos.x >= 0 && unit.SourcePrefab != null)
                {
                    battleTeam.Add(new RunManager.UnitPlacement
                    {
                        unitPrefab = unit.SourcePrefab,
                        row = pos.x,
                        col = pos.y
                    });
                }
            }
            RunManager.Instance.playerTeamPlacements = battleTeam;

            // Save bench team
            for (int i = 0; i < RunManager.Instance.playerBenchPlacements.Count; i++)
            {
                RunManager.Instance.playerBenchPlacements[i].unitPrefab = null;
            }

            // Now fill only the slots that have units on bench
            int benchIndex = 0;
            foreach (UnitInstance unit in benchGrid.GetAllUnits())
            {
                if (unit.SourcePrefab != null && benchIndex < RunManager.Instance.playerBenchPlacements.Count)
                {
                    RunManager.Instance.playerBenchPlacements[benchIndex].unitPrefab = unit.SourcePrefab;
                    benchIndex++;
                }
            }

            Debug.Log($"Saved: {battleTeam.Count} battle units, {benchIndex} bench units");
        }
    }

    void ClearAllUnits()
    {
        // Destroy all previously spawned units
        foreach (UnitInstance unit in spawnedUnits)
        {
            if (unit != null)
                Destroy(unit.gameObject);
        }
        spawnedUnits.Clear();

        // Clear grid references
        ClearGrid(battleGrid);
        ClearGrid(benchGrid);
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


    void AddCollidersToUnits()
    {
        foreach (UnitInstance unit in battleGrid.GetAllUnits())
        {
            if (unit.GetComponent<BoxCollider2D>() == null)
                unit.gameObject.AddComponent<BoxCollider2D>();
        }
        foreach (UnitInstance unit in benchGrid.GetAllUnits())
        {
            if (unit.GetComponent<BoxCollider2D>() == null)
                unit.gameObject.AddComponent<BoxCollider2D>();
        }
    }

}
