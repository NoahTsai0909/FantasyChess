// UnitSpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    private static UnitSpawner _instance;
    public static UnitSpawner Instance => _instance;

    [SerializeField]  private GridManager playerGrid;
    [SerializeField]  private GridManager enemyGrid;


    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    /*public void Initialize(GridManager playerGrid, GridManager enemyGrid)
    {
        this.playerGrid = playerGrid;
        this.enemyGrid = enemyGrid;
    }*/

    public UnitInstance SpawnUnit(
        UnitDefinition definition,
        int row,
        int col,
        bool isPlayer,
        UnitInstance spawnParent,
        Rarity? rarity = null,
        bool isSpawnedUnit = true
    )
    {
        // Get the appropriate grid
        GridManager targetGrid = isPlayer ? playerGrid : enemyGrid;

        // Check if position is valid
        if (!targetGrid.IsPositionValid(row, col))
        {
            Debug.LogError($"Invalid position ({row},{col}) for spawning unit");
            return null;
        }

        // Check if position is occupied
        if (targetGrid.IsPositionOccupied(row, col))
        {
            Debug.LogError($"Position ({row},{col}) is already occupied");
            return null;
        }

        // Instantiate the unit
        UnitInstance unit = Instantiate(definition.unitPrefab);

        // Initialize based on player or enemy
        if (isPlayer)
        {
            // For player units, you might need to create save data
            // or initialize differently
            UnitSaveData saveData = new UnitSaveData
            {
                definition = definition,
                rarity = rarity ?? Rarity.Common,
                id = System.Guid.NewGuid()
            };
            unit.InitializeFromSaveData(saveData);
        }
        else
        {
            unit.InitializeEnemy(definition, rarity ?? Rarity.Common);
        }

        // Mark as spawned unit (important for persistence/save/load)
        unit.isSpawnedUnit = true;
        unit.spawnSource = spawnParent; // Track what spawned it

        // Enter combat at the specified position
        unit.EnterCombat(targetGrid, row, col, isPlayer);

        // Trigger any spawn events
        CombatEventBus.Publish(
            CombatEventBus.CombatEventType.UnitSpawned,
            unit,
            null,
            0
        );

        return unit;
    }

    public bool TryFindSpawnPosition(int originalRow, int originalCol, bool isPlayer, out int spawnRow, out int spawnCol)
    {
        GridManager targetGrid = isPlayer ? playerGrid : enemyGrid;

        // Try original position first
        if (targetGrid.IsPositionValid(originalRow, originalCol) &&
            !targetGrid.IsPositionOccupied(originalRow, originalCol))
        {
            spawnRow = originalRow;
            spawnCol = originalCol;
            return true;
        }

        // Try adjacent positions
        Vector2Int[] offsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Right
            new Vector2Int(0, -1), // Left
            new Vector2Int(1, 0),  // Up
            new Vector2Int(-1, 0)  // Down
        };

        foreach (var offset in offsets)
        {
            int testRow = originalRow + offset.x;
            int testCol = originalCol + offset.y;

            if (targetGrid.IsPositionValid(testRow, testCol) &&
                !targetGrid.IsPositionOccupied(testRow, testCol))
            {
                spawnRow = testRow;
                spawnCol = testCol;
                return true;
            }
        }

        spawnRow = -1;
        spawnCol = -1;
        return false;
    }
}
