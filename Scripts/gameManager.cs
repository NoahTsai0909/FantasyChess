using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{

    [Header("Grids")]
    public GridManager playerGrid;
    public GridManager enemyGrid;

    [Header("UI Manager")]
    [SerializeField] private BattleUIManager battleUIManager;

    [Header("Current Battle")]
    [SerializeField] private TeamDefinition playerTeam;
    [SerializeField] private EncounterDefinition currentEncounter;

    void Start()
    {
        if (playerTeam != null && currentEncounter != null)
        {
            InitializeBattlefield(playerTeam, currentEncounter);
        }
        if (battleUIManager != null)
        {
            battleUIManager.Initialize(playerGrid, enemyGrid);
        }
    }

    public void InitializeBattlefield(TeamDefinition playerTeam, EncounterDefinition encounter)
    {
        // Spawn player team
        foreach (var unitPlacement in playerTeam.units)
        {
            if (unitPlacement.unitPrefab != null)
            {
                SpawnUnit(unitPlacement.unitPrefab, playerGrid,
                         unitPlacement.row, unitPlacement.col, true);
            }
        }

        // Spawn enemies
        foreach (var unitPlacement in encounter.enemyUnits)
        {
            if (unitPlacement.unitPrefab != null)
            {
                SpawnUnit(unitPlacement.unitPrefab, enemyGrid,
                         unitPlacement.row, unitPlacement.col, false);
            }
        }
    }

    private UnitInstance SpawnUnit(UnitInstance unitPrefab, GridManager grid,
                                   int row, int col, bool isPlayer)
    {
        // 1. Instantiate the SPECIFIC prefab (BannerKnight, SolemnPriest, etc.)
        UnitInstance unit = Instantiate(unitPrefab);

        // 2. Set team
        unit.isPlayer = isPlayer;

        // 3. Initialize with UI prefabs
        unit.Initialize(grid, row, col);

        return unit;
    }
}
