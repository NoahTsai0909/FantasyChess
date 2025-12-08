using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 3;
    public int cols = 3;
    public float cellSize = 2f;

    public GameObject tilePrefab;   // <-- you create this (a simple colored square tile)

    private Vector2[,] worldPositions;
    private UnitInstance[,] gridUnits;

    void Awake()
    {
        worldPositions = new Vector2[rows, cols];
        gridUnits = new UnitInstance[rows, cols];

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
        Debug.Log($"CreateVisualTiles called on {gameObject.name} | tilePrefab = {tilePrefab}");

        // Destroy old children (if reloading)
        foreach (Transform t in transform)
            DestroyImmediate(t.gameObject);

        // Create tiles
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.position = worldPositions[r, c];
                tile.transform.localScale = Vector3.one * (cellSize * 0.95f);
            }
        }
    }

    public bool PlaceUnit(UnitInstance unit, int r, int c)
    {
        if (!InBounds(r, c)) return false;
        if (gridUnits[r, c] != null) return false; // cell occupied

        gridUnits[r, c] = unit;
        unit.transform.position = worldPositions[r, c];
        return true;
    }

    public bool InBounds(int r, int c)
    {
        return r >= 0 && r < rows && c >= 0 && c < cols;
    }

    public List<UnitInstance> GetAllUnits()
    {
        List<UnitInstance> units = new List<UnitInstance>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (gridUnits[r, c] != null && gridUnits[r, c].gameObject != null)
                {
                    units.Add(gridUnits[r, c]);
                }
                else
                {
                    // Clean up null references
                    gridUnits[r, c] = null;
                }
            }
        }

        return units;
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

    public void RemoveUnit(int r, int c)
    {
        if (InBounds(r, c))
        {
            gridUnits[r, c] = null;
        }
    }

    public Vector2Int GetUnitPosition(UnitInstance unit)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (gridUnits[r, c] == unit)
                {
                    return new Vector2Int(r, c);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public Vector2 GetWorldPosition(int row, int col)
    {
        if (InBounds(row, col))
            return worldPositions[row, col];
        return transform.position;
    }

    public bool IsCellEmpty(int r, int c)
    {
        return InBounds(r, c) && gridUnits[r, c] == null;
    }

    public float DistanceToNearestEmptyCell(Vector3 worldPosition)
    {
        float minDistance = float.MaxValue;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (IsCellEmpty(r, c)) // Only consider empty cells
                {
                    float dist = Vector2.Distance(worldPosition, GetWorldPosition(r, c));
                    if (dist < minDistance)
                        minDistance = dist;
                }
            }
        }

        return minDistance;
    }

    public void ClearAllUnits()
    {
        gridUnits = new UnitInstance[rows, cols];
    }

    public UnitInstance GetUnitAtPosition(int row, int col)
    {
        if (InBounds(row, col))
            return gridUnits[row, col];
        return null;
    }
}
