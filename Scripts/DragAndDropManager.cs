using UnityEngine;
using UnityEngine.InputSystem;
using static RunManager;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField] private GridManager battleGrid;
    [SerializeField] private GridManager benchGrid;

    private UnitInstance draggedUnit;
    private RunManager.UnitPlacement draggedPlacement;
    private GridManager sourceGrid;
    private Vector2Int sourcePos;

    private Camera mainCamera;
    private Mouse mouse;
    private bool wasMouseDown = false;

    void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;

        if (battleGrid == null || benchGrid == null)
            Debug.LogError("DragAndDropManager: Grid references not set!");
    }

    void Update()
    {
        if (mouse == null) return;

        bool isMouseDown = mouse.leftButton.isPressed;

        if (isMouseDown && !wasMouseDown)
            TryStartDrag();

        if (!isMouseDown && wasMouseDown && draggedUnit != null)
            StopDrag();

        if (draggedUnit != null && isMouseDown)
            draggedUnit.transform.position = GetMouseWorldPosition();

        wasMouseDown = isMouseDown;
    }

    void TryStartDrag()
    {
        Vector3 worldPos = GetMouseWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null) return;

        UnitInstance unit = hit.GetComponent<UnitInstance>();
        if (unit == null || unit.myPlacement == null) return;

        GridManager grid = GetUnitGrid(unit);
        if (grid == null) return;

        StartDrag(unit, grid);
    }

    GridManager FindUnitGrid(UnitInstance unit)
    {
        if (battleGrid.GetUnitPosition(unit) != new Vector2Int(-1, -1))
            return battleGrid;
        if (benchGrid.GetUnitPosition(unit) != new Vector2Int(-1, -1))
            return benchGrid;
        return null;
    }

    GridManager GetUnitGrid(UnitInstance unit)
    {
        if (battleGrid.GetUnitPosition(unit) != new Vector2Int(-1, -1))
            return battleGrid;
        if (benchGrid.GetUnitPosition(unit) != new Vector2Int(-1, -1))
            return benchGrid;
        return null;
    }

    void StartDrag(UnitInstance unit, GridManager grid)
    {
        draggedUnit = unit;
        draggedPlacement = unit.myPlacement;
        sourceGrid = grid;
        sourcePos = grid.GetUnitPosition(unit);

        // Remove unit from source grid, but don't destroy visual
        sourceGrid.RemoveUnit(sourcePos.x, sourcePos.y, destroyVisual: false);

        SetUnitDragVisuals(unit, true);
    }

    void StopDrag()
    {
        Vector3 dropPos = GetMouseWorldPosition();
        GridManager targetGrid = GetClosestGrid(dropPos);
        Vector2Int targetPos = targetGrid.GetNearestGridPosition(dropPos);

        // Get target unit if any
        UnitInstance targetUnit = targetGrid.GetUnitAtPosition(targetPos.x, targetPos.y);
        RunManager.UnitPlacement targetPlacement = targetUnit != null ? targetUnit.myPlacement : null;

        // Case 1: target cell occupied = swap
        if (targetUnit != null)
        {
            // Put target unit into source cell
            sourceGrid.PlaceUnit(targetPlacement, sourcePos.x, sourcePos.y, targetUnit);
            targetPlacement.row = sourcePos.x;
            targetPlacement.col = sourcePos.y;
        }

        // Update dragged unit placement
        draggedPlacement.row = targetPos.x;
        draggedPlacement.col = targetPos.y;

        // Case 2: moving between grids or within same grid
        // Remove dragged unit from source grid reference (visual already gone in StartDrag)
        sourceGrid.ClearUnitReference(draggedPlacement);

        // Place dragged unit in target grid
        targetGrid.PlaceUnit(draggedPlacement, targetPos.x, targetPos.y, draggedUnit);

        SetUnitDragVisuals(draggedUnit, false);

        // Clear drag state
        draggedUnit = null;
        draggedPlacement = null;
        sourceGrid = null;
    }




    GridManager GetClosestGrid(Vector3 worldPos)
    {
        float distBattle = battleGrid.DistanceToNearestEmptyCell(worldPos);
        float distBench = benchGrid.DistanceToNearestEmptyCell(worldPos);
        return (distBattle < distBench) ? battleGrid : benchGrid;
    }


    GridManager ChooseTargetGrid(Vector3 dropPosition)
    {
        // Simple: choose closest grid center
        float distToBattle = Vector2.Distance(dropPosition, battleGrid.transform.position);
        float distToBench = Vector2.Distance(dropPosition, benchGrid.transform.position);

        return (distToBattle < distToBench) ? battleGrid : benchGrid;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 pos = mouse.position.ReadValue();
        pos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(pos);
    }

    void SetUnitDragVisuals(UnitInstance unit, bool isDragging)
    {
        SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = isDragging ? 0.6f : 1f;
            sr.color = c;
        }
    }

    GridManager ChooseTargetGridByCellDistance(Vector3 dropPosition)
    {
        float distToBattle = battleGrid.DistanceToNearestEmptyCell(dropPosition);
        float distToBench = benchGrid.DistanceToNearestEmptyCell(dropPosition);

        return (distToBattle < distToBench) ? battleGrid : benchGrid;
    }

    Vector2Int FindNearestEmptyCell(GridManager grid, Vector3 position)
    {
        Vector2Int nearest = new Vector2Int(-1, -1);
        float closestDist = float.MaxValue;

        for (int r = 0; r < grid.rows; r++)
        {
            for (int c = 0; c < grid.cols; c++)
            {
                if (grid.IsCellEmpty(r, c))
                {
                    Vector2 cellPos = grid.GetWorldPosition(r, c);
                    float dist = Vector2.Distance(position, cellPos);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        nearest = new Vector2Int(r, c);
                    }
                }
            }
        }

        return nearest;
    }

    Vector2Int FindUnitPosition(UnitInstance unit, GridManager grid)
    {
        return grid.GetUnitPosition(unit);
    }
}