using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static RunManager;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField] private GridManager battleGrid;
    [SerializeField] private GridManager benchGrid;
    [SerializeField] private SellZone sellZone;

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

        // Update sell zone highlight based on mouse position
        if (draggedUnit != null)
        {
            Vector3 worldPos = GetMouseWorldPosition();
            bool overSellZone = IsInSellZone(worldPos);

            // Highlight the sell zone visual
            if (sellZone != null)
                sellZone.Highlight(overSellZone);

            // Change unit color when over sell zone
            SpriteRenderer sr = draggedUnit.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = overSellZone ? Color.red : Color.white;
            }
        }

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

        // Bring unit to front when dragging
        SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 100; // High number to bring to front
        }
    }

    void StopDrag()
    {
        Vector3 dropPos = GetMouseWorldPosition();

        if (IsInSellZone(dropPos))
        {
            SellUnit();
        }
        else
        {
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
        }

        SetUnitDragVisuals(draggedUnit, false);

        // Reset sorting order
        if (draggedUnit != null)
        {
            SpriteRenderer sr = draggedUnit.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 0; // Reset to default
            }
        }

        // Reset sell zone highlight
        if (sellZone != null)
            sellZone.Highlight(false);

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

    bool IsInSellZone(Vector3 worldPos)
    {
        if (sellZone == null) return false;

        Collider2D sellCollider = sellZone.GetComponent<Collider2D>();
        if (sellCollider == null) return false;

        return sellCollider.OverlapPoint(worldPos);
    }

    void SellUnit()
    {
        if (draggedUnit == null || draggedPlacement == null) return;

        Debug.Log($"Selling unit: {draggedUnit.unitName}");

        // Calculate sell price
        int sellPrice = draggedUnit.Definition.cost;

        // Add gold to player
        RunManager.Instance.currentGold += sellPrice;
        Debug.Log($"Gained {sellPrice} gold. Total: {RunManager.Instance.currentGold}");

        // Remove from source grid
        if (sourceGrid != null)
        {
            sourceGrid.RemoveUnit(sourcePos.x, sourcePos.y, destroyVisual: true);
        }

        // Remove from RunManager's data
        RemoveFromRunManager(draggedUnit, draggedPlacement);

        // Destroy the unit GameObject
        Destroy(draggedUnit.gameObject);

        RunManager.Instance.currentGold += sellPrice;
    }

    void RemoveFromRunManager(UnitInstance unit, UnitPlacement placement)
    {
        if (RunManager.Instance == null) return;

        // Check if unit is in battle grid or bench
        bool isInBattleGrid = sourceGrid == battleGrid;

        if (isInBattleGrid)
        {
            // Remove from playerTeamPlacements
            RunManager.Instance.playerTeamPlacements.RemoveAll(p =>
                p.unitData == placement.unitData);
        }
        else
        {
            // Remove from playerBenchPlacements
            foreach (var benchPlacement in RunManager.Instance.playerBenchPlacements)
            {
                if (benchPlacement.unitData == placement.unitData)
                {
                    benchPlacement.unitData = null;
                    benchPlacement.row = -1;
                    benchPlacement.col = -1;
                    break;
                }
            }
        }
    }
}