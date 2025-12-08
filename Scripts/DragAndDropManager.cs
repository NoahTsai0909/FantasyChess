using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField] private GridManager battleGrid;
    [SerializeField] private GridManager benchGrid;

    private UnitInstance draggedUnit;
    private GridManager sourceGrid;
    private Vector2Int sourcePosition;
    private Camera mainCamera;

    private Mouse mouse;
    private bool isMouseDown = false;
    private bool wasMouseDown = false;

    void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
        // Safety check
        if (battleGrid == null || benchGrid == null)
            Debug.LogError("DragAndDropManager: Grid references not set!");
    }

    void Update()
    {
        if (mouse == null) return;
        isMouseDown = mouse.leftButton.isPressed;

        if (isMouseDown && !wasMouseDown)
            TryStartDrag();

        // Mouse button released
        if (!isMouseDown && wasMouseDown && draggedUnit != null)
            StopDrag();

        // Update dragged unit position
        if (draggedUnit != null && isMouseDown)
            draggedUnit.transform.position = GetMouseWorldPosition();

        wasMouseDown = isMouseDown;
    }

    void TryStartDrag()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null)
        {
            UnitInstance unit = hit.GetComponent<UnitInstance>();
            if (unit != null)
            {
                GridManager sourceGrid = FindUnitGrid(unit);
                if (sourceGrid != null)
                    StartDrag(unit, sourceGrid);
            }
        }
    }

    GridManager FindUnitGrid(UnitInstance unit)
    {
        // Check battle grid
        if (battleGrid.GetUnitPosition(unit).x >= 0)
            return battleGrid;

        // Check bench grid
        if (benchGrid.GetUnitPosition(unit).x >= 0)
            return benchGrid;

        return null;
    }

    void StartDrag(UnitInstance unit, GridManager foundGrid)
    {
        draggedUnit = unit;
        sourceGrid = foundGrid; // Store the source grid
        sourcePosition = sourceGrid.GetUnitPosition(unit);

        // CRITICAL: Remove unit from source grid while dragging
        sourceGrid.RemoveUnit(sourcePosition.x, sourcePosition.y);

        // Visual feedback
        SetUnitDragVisuals(unit, true);

        Debug.Log($"Started dragging {unit.unitName} from {sourceGrid.name} at ({sourcePosition.x}, {sourcePosition.y})");
    }

    void StopDrag()
    {
        if (draggedUnit == null || sourceGrid == null) return;

        Vector3 dropPos = GetMouseWorldPosition();
        GridManager targetGrid = ChooseTargetGridByCellDistance(dropPos);
        Vector2Int targetPos = targetGrid.GetNearestGridPosition(dropPos);

        // Skip if dropping in same spot
        if (targetGrid == sourceGrid && targetPos == sourcePosition)
        {
            sourceGrid.PlaceUnit(draggedUnit, sourcePosition.x, sourcePosition.y);
        }
        else
        {
            // Get unit at target position (if any)
            UnitInstance targetUnit = targetGrid.GetUnitAtPosition(targetPos.x, targetPos.y);

            // Remove dragged unit from source
            sourceGrid.RemoveUnit(sourcePosition.x, sourcePosition.y);

            if (targetUnit != null)
            {
                // Remove target unit from target grid
                targetGrid.RemoveUnit(targetPos.x, targetPos.y);

                // Place target unit in source position
                sourceGrid.PlaceUnit(targetUnit, sourcePosition.x, sourcePosition.y);
            }

            // Place dragged unit in target position
            targetGrid.PlaceUnit(draggedUnit, targetPos.x, targetPos.y);
        }

        SetUnitDragVisuals(draggedUnit, false);
        draggedUnit = null;
        sourceGrid = null;
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
        if (mouse == null) return Vector3.zero;

        Vector3 mousePos = mouse.position.ReadValue();
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    void SetUnitDragVisuals(UnitInstance unit, bool isDragging)
    {
        SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a = isDragging ? 0.6f : 1f;
            sr.color = color;
        }
    }

    GridManager ChooseTargetGridByCellDistance(Vector3 dropPosition)
    {
        // Calculate distance to nearest empty cell in EACH grid
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