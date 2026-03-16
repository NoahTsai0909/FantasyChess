using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UnitHoverDetector : MonoBehaviour
{
    [SerializeField] private UnitHoverUI hoverUIPrefab;
    [SerializeField] private float hoverDelay = 0.35f;

    private Camera mainCamera;
    private Mouse mouse;

    private UnitInstance currentHoveredUnit;
    private UnitInstance pendingHoverUnit;
    private Coroutine hoverRoutine;

    private UnitHoverUI hoverUIInstance;

    void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;

        // Find the Screen Space - Overlay canvas (or the one you want)
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas targetCanvas = null;

        foreach (Canvas canvas in canvases)
        {
            // Choose based on render mode
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                targetCanvas = canvas;
                Debug.Log($"Found Screen Space Canvas: {canvas.name}");
                break;
            }
        }

        // Fallback to first canvas if none found
        if (targetCanvas == null && canvases.Length > 0)
        {
            targetCanvas = canvases[0];
            Debug.LogWarning($"No Screen Space canvas found, using: {targetCanvas.name}");
        }

        if (targetCanvas == null)
        {
            Debug.LogError("No canvas found in scene! Unit hover UI cannot be created.");
            return;
        }

        // Instantiate the prefab as a child of the target canvas
        hoverUIInstance = Instantiate(hoverUIPrefab, targetCanvas.transform);
        hoverUIInstance.gameObject.SetActive(false);
        hoverUIInstance.name = "UnitUI (Dynamic)";
    }

    void Update()
    {
        if (mouse == null) return;

        // Do not show hover while dragging
        if (mouse.leftButton.isPressed)
        {
            CancelHover();
            return;
        }

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

        UnitInstance hitUnit = hit ? hit.GetComponent<UnitInstance>() : null;

        // No unit under mouse
        if (hitUnit == null)
        {
            CancelHover();
            return;
        }

        // Same unit already hovered do nothing
        if (hitUnit == currentHoveredUnit || hitUnit == pendingHoverUnit)
            return;

        // New unit hovered
        StartHover(hitUnit);
    }

    void StartHover(UnitInstance unit)
    {
        CancelHover(); // stop previous hover attempt

        pendingHoverUnit = unit;
        hoverRoutine = StartCoroutine(HoverDelayRoutine(unit));
    }

    IEnumerator HoverDelayRoutine(UnitInstance unit)
    {
        yield return new WaitForSeconds(hoverDelay);

        // Ensure mouse is STILL over the same unit
        if (pendingHoverUnit == unit)
        {
            currentHoveredUnit = unit;
            pendingHoverUnit = null;
            hoverUIInstance.Show(unit);
        }
    }

    void CancelHover()
    {
        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        hoverRoutine = null;
        pendingHoverUnit = null;

        if (currentHoveredUnit != null)
        {
            hoverUIInstance.Hide();
            currentHoveredUnit = null;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = mouse.position.ReadValue();
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    void OnDestroy()
    {
        if (hoverUIInstance != null)
        {
            Destroy(hoverUIInstance.gameObject);
        }
    }
}