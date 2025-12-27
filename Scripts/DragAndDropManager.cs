using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static RunManager;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField] private GridManager battleGrid;
    [SerializeField] private GridManager benchGrid;
    [SerializeField] private SellZone sellZone;
    [SerializeField] private ProvisionManager provisionManager;

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
        if (provisionManager == null)
            provisionManager = FindFirstObjectByType<ProvisionManager>();
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

            // CASE 1: Moving to empty cell
            if (targetUnit == null)
            {
                // Only check provision if moving TO battle grid FROM bench
                if (targetGrid == battleGrid && sourceGrid == benchGrid)
                {
                    if (provisionManager != null && !provisionManager.CanAddUnitToBattleGrid(draggedUnit))
                    {
                        RevertDrag();
                        return;
                    }
                }
                // If moving within same grid or from battle to bench, no provision check needed
            }
            // CASE 2: Swapping units
            else
            {
                // Determine the grid relationship
                bool sameGrid = sourceGrid == targetGrid;
                bool movingToBattle = targetGrid == battleGrid;
                bool movingToBench = targetGrid == benchGrid;
                bool fromBattle = sourceGrid == battleGrid;
                bool fromBench = sourceGrid == benchGrid;

                // Different provision scenarios:
                if (sameGrid)
                {
                    // Swapping within same grid - no provision change needed
                    // (Total provision stays the same)
                }
                else if (movingToBattle && fromBench)
                {
                    // Swapping bench unit for battle unit
                    if (provisionManager != null && !provisionManager.CanSwapUnits(targetUnit, draggedUnit))
                    {
                        RevertDrag();
                        return;
                    }
                }
                else if (movingToBench && fromBattle)
                {
                    // Swapping battle unit for bench unit
                    if (provisionManager != null && !provisionManager.CanSwapUnits(draggedUnit, targetUnit))
                    {
                        RevertDrag();
                        return;
                    }
                }
            }

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

        // Reset sell zone highlight
        if (sellZone != null)
            sellZone.Highlight(false);

        // Clear drag state
        draggedUnit = null;
        draggedPlacement = null;
        sourceGrid = null;

        if (provisionManager != null)
            provisionManager.CalculateCurrentProvision();
    }

    void RevertDrag()
    {
        // Return unit to source position
        sourceGrid.PlaceUnit(draggedPlacement, sourcePos.x, sourcePos.y, draggedUnit);
        draggedPlacement.row = sourcePos.x;
        draggedPlacement.col = sourcePos.y;

        // Reset visuals
        SetUnitDragVisuals(draggedUnit, false);

        SpriteRenderer sr = draggedUnit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 0;
        }

        // Show feedback
        StartCoroutine(ShowProvisionWarning());

        // Clear drag state
        draggedUnit = null;
        draggedPlacement = null;
        sourceGrid = null;
    }

    System.Collections.IEnumerator ShowProvisionWarning()
    {
        var provisionText = provisionManager.provisionText;
        if (provisionText != null)
        {
            Color originalColor = provisionText.color;
            provisionText.color = provisionManager.exceededColor;
            yield return new WaitForSeconds(0.5f);
            provisionText.color = originalColor;
        }
    }

    GridManager GetClosestGrid(Vector3 worldPos)
    {
        // Calculate distance to grid centers instead of nearest empty cell
        float distToBattle = Vector2.Distance(worldPos, battleGrid.transform.position);
        float distToBench = Vector2.Distance(worldPos, benchGrid.transform.position);

        // Or use a weighted distance that considers grid bounds
        float battleDist = Mathf.Min(
            distToBattle,
            battleGrid.DistanceToNearestEmptyCell(worldPos)
        );

        float benchDist = Mathf.Min(
            distToBench,
            benchGrid.DistanceToNearestEmptyCell(worldPos)
        );

        return (battleDist < benchDist) ? battleGrid : benchGrid;
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