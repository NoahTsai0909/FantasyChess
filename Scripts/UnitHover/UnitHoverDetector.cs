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

    private bool isPinned = false;

    void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;

        // 1. Create a bulletproof, dedicated canvas just for tooltips
        GameObject tooltipCanvasObj = new GameObject("UnitHoverCanvas");
        Canvas tooltipCanvas = tooltipCanvasObj.AddComponent<Canvas>();

        // 2. Force it to be an overlay with a massive sorting order
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tooltipCanvas.sortingOrder = 30000;

        // 3. Add and CONFIGURE the CanvasScaler so it matches your game's resolution
        UnityEngine.UI.CanvasScaler scaler = tooltipCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        tooltipCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 4. Instantiate the prefab as a child of this new top-level canvas
        hoverUIInstance = Instantiate(hoverUIPrefab, tooltipCanvas.transform);
        hoverUIInstance.gameObject.SetActive(false);
        hoverUIInstance.name = "UnitUI (Dynamic)";
    }

    void Update()
    {
        if (mouse == null || Keyboard.current == null) return;

        if (isPinned)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame ||
                Keyboard.current.escapeKey.wasPressedThisFrame ||
                mouse.leftButton.wasPressedThisFrame ||
                mouse.rightButton.wasPressedThisFrame)
            {
                isPinned = false;
                CancelHover();
            }
            return;
        }
        if (currentHoveredUnit != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            isPinned = true;
            return;
        }

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
        if (isPinned) return;

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