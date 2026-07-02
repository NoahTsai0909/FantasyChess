using System.Collections.Generic;
using UnityEngine;
using static RunManager;

public class GridManager : MonoBehaviour
{
    public int rows = 3;
    public int cols = 3;
    public float cellSize = 4f;

    public GameObject tilePrefab;   // <-- you create this (a simple colored square tile)

    private Vector2[,] worldPositions;
    private RunManager.UnitPlacement[,] gridPlacements;
    private UnitInstance[,] unitInstances;
    private Vector2 unitVisualOffset = new Vector2(0f, 1f);


    void Awake()
    {
        worldPositions = new Vector2[rows, cols];
        gridPlacements = new RunManager.UnitPlacement[rows, cols];
        unitInstances = new UnitInstance[rows, cols];

        GenerateGrid();
        CreateVisualTiles();
    }

    void GenerateGrid()
    {
        float halfW = (cols - 1) * cellSize * 0.5f;
        float halfH = (rows - 1) * cellSize * 0.5f;
        Vector2 center = transform.position;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = center.x + (c * cellSize) - halfW;
                float y = center.y - (r * cellSize) + halfH;
                worldPositions[r, c] = new Vector2(x, y);
            }
        }
    }


    void CreateVisualTiles()
    {
        foreach (Transform t in transform)
            DestroyImmediate(t.gameObject);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.position = worldPositions[r, c];
                /*tile.transform.localScale = Vector3.one * (cellSize * 0.95f);*/
            }
        }
    }


    // ------------------- UNIT PLACEMENTS -------------------

    public bool PlaceUnit(UnitPlacement placement, int r, int c, UnitInstance instance = null)
    {
        return PlaceUnit(placement, r, c, instance, true); // Default to player side
    }

    // New overload with isPlayer parameter
    public bool PlaceUnit(UnitPlacement placement, int r, int c, UnitInstance instance = null, bool isPlayer = true)
    {
        if (!InBounds(r, c)) return false;

        // Only destroy old unit if instance == null (i.e., we are spawning new)
        if (instance == null && unitInstances[r, c] != null)
            Destroy(unitInstances[r, c].gameObject);

        gridPlacements[r, c] = placement;

        if (instance == null)
        {
            UnitInstance prefab = placement.unitData.definition.unitPrefab;
            instance = Instantiate(prefab, GetUnitWorldPosition(r, c), Quaternion.identity);
            if (isPlayer)
            {
                instance.InitializeFromSaveData(placement.unitData);
            }
            else
            {
                instance.InitializeEnemy(placement.unitData.definition, placement.unitData.rarity);
            }
        }

        // Set visual position & link to placement
        instance.transform.position = GetUnitWorldPosition(r, c);

        instance.myPlacement = placement;

        // Set player side
        instance.SetPlayerSide(isPlayer);

        unitInstances[r, c] = instance;

        return true;
    }

    public bool PlaceUnit(int r, int c, UnitInstance instance, bool isPlayer)
    {
        if (!InBounds(r, c)) return false;

        instance.transform.position = GetUnitWorldPosition(r, c);

        instance.SetPlayerSide(isPlayer);

        unitInstances[r, c] = instance;

        return true;
    }





    public void RemoveUnit(int r, int c, bool destroyVisual = true)
    {
        if (!InBounds(r, c)) return;

        gridPlacements[r, c] = null;

        if (unitInstances[r, c] != null)
        {
            if (destroyVisual)
                Destroy(unitInstances[r, c].gameObject);

            unitInstances[r, c] = null;
        }
    }

    public void ClearUnitReference(UnitPlacement placement)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (gridPlacements[r, c] == placement)
                {
                    gridPlacements[r, c] = null;
                    unitInstances[r, c] = null;
                    return;
                }
            }
        }
    }


    public RunManager.UnitPlacement GetUnitPlacement(int r, int c)
    {
        if (!InBounds(r, c)) return null;
        return gridPlacements[r, c];
    }

    public UnitInstance GetUnitAtPosition(int r, int c)
    {
        if (!InBounds(r, c)) return null;
        return unitInstances[r, c];
    }

    public bool InBounds(int r, int c)
    {
        return r >= 0 && r < rows && c >= 0 && c < cols;
    }

    public Vector2Int GetUnitPosition(UnitInstance unit)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (unitInstances[r, c] == unit)
                    return new Vector2Int(r, c);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public List<UnitInstance> GetAllUnits()
    {
        List<UnitInstance> units = new List<UnitInstance>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (unitInstances[r, c] != null)
                    units.Add(unitInstances[r, c]);
        return units;
    }

    public UnitInstance GetRandomUnit()
    {
        List<UnitInstance> units = GetAllUnits();

        if (units.Count == 0)
            return null;

        int randomIndex = Random.Range(0, units.Count);
        return units[randomIndex];
    }

    public void ClearAllUnits()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                RemoveUnit(r, c);
    }

    public Vector2Int GetNearestGridPosition(Vector3 worldPosition)
    {
        Vector2Int nearest = new Vector2Int(-1, -1);
        float closestDist = float.MaxValue;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float dist = Vector2.Distance(worldPosition, worldPositions[r, c]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nearest = new Vector2Int(r, c);
                }
            }
        }

        return nearest;
    }

    public bool IsCellEmpty(int r, int c)
    {
        return InBounds(r, c) && gridPlacements[r, c] == null;
    }

    public float DistanceToNearestEmptyCell(Vector3 worldPos)
    {
        float minDist = float.MaxValue;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (IsCellEmpty(r, c))
                    minDist = Mathf.Min(minDist, Vector2.Distance(worldPos, worldPositions[r, c]));
        return minDist;
    }

    public Vector2 GetWorldPosition(int row, int col)
    {
        if (InBounds(row, col))
            return worldPositions[row, col];
        return transform.position;
    }

    public Vector2 GetCellWorldPosition(int r, int c)
    {
        if (!InBounds(r, c)) return transform.position;
        return worldPositions[r, c];
    }

    public Vector2 GetUnitWorldPosition(int r, int c)
    {
        if (!InBounds(r, c)) return transform.position;
        return worldPositions[r, c] + unitVisualOffset;
    }

    public bool IsPositionValid(int row, int col)
    {
        return InBounds(row, col);
    }


    public bool IsPositionOccupied(int row, int col)
    {
        if (!InBounds(row, col))
            return true; // Out of bounds is considered occupied

        return unitInstances[row, col] != null;
    }

    public UnitInstance GetUnitAt(int row, int col)
    {
        if (!InBounds(row, col))
            return null;

        return unitInstances[row, col];
    }
}
