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
    private CanvasGroup hoverUICanvasGroup; // ADDED: To control raycasts dynamically

    private bool isPinned = false;

    public static UnitHoverDetector Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;

        GameObject tooltipCanvasObj = new GameObject("UnitHoverCanvas");
        Canvas tooltipCanvas = tooltipCanvasObj.AddComponent<Canvas>();
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tooltipCanvas.sortingOrder = 30000;

        UnityEngine.UI.CanvasScaler scaler = tooltipCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        tooltipCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        hoverUIInstance = Instantiate(hoverUIPrefab, tooltipCanvas.transform);
        hoverUIInstance.gameObject.SetActive(false);
        hoverUIInstance.name = "UnitUI (Dynamic)";

        // ADDED: Cache the CanvasGroup so we can toggle its raycasts
        hoverUICanvasGroup = hoverUIInstance.GetComponent<CanvasGroup>();
        if (hoverUICanvasGroup == null)
        {
            hoverUICanvasGroup = hoverUIInstance.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (mouse == null || Keyboard.current == null) return;

        // 1. Unpinning Logic
        if (isPinned)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame ||
                Keyboard.current.escapeKey.wasPressedThisFrame ||
                mouse.leftButton.wasPressedThisFrame ||
                mouse.rightButton.wasPressedThisFrame)
            {
                isPinned = false;
                if (hoverUICanvasGroup != null) hoverUICanvasGroup.blocksRaycasts = false; // Turn OFF raycasts
                CancelHover();
            }
            return;
        }

        // 2. Pinning Logic
        if (currentHoveredUnit != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            isPinned = true;
            if (hoverUICanvasGroup != null) hoverUICanvasGroup.blocksRaycasts = true; // Turn ON raycasts for links!
            return;
        }

        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (mouse.leftButton.isPressed)
        {
            CancelHover();
            return;
        }

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        UnitInstance hitUnit = hit ? hit.GetComponent<UnitInstance>() : null;

        if (hitUnit == null)
        {
            CancelHover();
            return;
        }

        if (hitUnit == currentHoveredUnit || hitUnit == pendingHoverUnit) return;

        StartHover(hitUnit);
    }

    void StartHover(UnitInstance unit)
    {
        CancelHover();

        // Ensure raycasts are OFF during normal board hover
        if (hoverUICanvasGroup != null) hoverUICanvasGroup.blocksRaycasts = false;

        pendingHoverUnit = unit;
        hoverRoutine = StartCoroutine(HoverDelayRoutine(unit));
    }

    IEnumerator HoverDelayRoutine(UnitInstance unit)
    {
        yield return new WaitForSeconds(hoverDelay);
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

        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
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
        if (hoverUIInstance != null) Destroy(hoverUIInstance.gameObject);
    }

    // Called by the ShopUnitCard
    public void ShowTooltipFromUI(UnitInstance unit)
    {
        if (isPinned) return;

        CancelHover();
        currentHoveredUnit = unit;

        // Ensure raycasts are OFF so the Shop Card doesn't flicker!
        if (hoverUICanvasGroup != null) hoverUICanvasGroup.blocksRaycasts = false;

        hoverUIInstance.Show(unit);
    }

    // Called by the ShopUnitCard
    public void HideTooltipFromUI()
    {
        if (!isPinned) CancelHover();
    }
}