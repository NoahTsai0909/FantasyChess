using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TacticHoverDetector : MonoBehaviour
{
    // --- NEW: Singleton Instance ---
    public static TacticHoverDetector Instance { get; private set; }

    [SerializeField] private float hoverDelay = 0.35f;

    private Camera mainCamera;
    private Mouse mouse;

    private TacticInstance currentHoveredTactic;
    private TacticInstance pendingHoverTactic;
    private Coroutine hoverRoutine;

    // --- NEW: UI Safety Flag ---
    private bool isUIHoverDriven = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
    }

    void Update()
    {
        if (mouse == null) return;

        // --- NEW: If the UI is currently showing a tooltip, stop checking physics! ---
        if (isUIHoverDriven) return;

        // If the player clicks (like dragging a tactic), cancel the hover
        if (mouse.leftButton.isPressed)
        {
            CancelHover();
            return;
        }

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        TacticInstance hitTactic = hit ? hit.GetComponent<TacticInstance>() : null;

        // Not hovering a tactic
        if (hitTactic == null)
        {
            CancelHover();
            return;
        }

        if (hitTactic == currentHoveredTactic)
        {
            if (TooltipUIManager.Instance != null)
            {
                TooltipUIManager.Instance.UpdatePosition(mouse.position.ReadValue());
            }
            return;
        }

        if (hitTactic == pendingHoverTactic) return;

        StartHover(hitTactic);
    }

    void StartHover(TacticInstance tactic)
    {
        CancelHover();
        pendingHoverTactic = tactic;
        hoverRoutine = StartCoroutine(HoverDelayRoutine(tactic));
    }

    IEnumerator HoverDelayRoutine(TacticInstance tactic)
    {
        yield return new WaitForSeconds(hoverDelay);
        if (pendingHoverTactic == tactic)
        {
            currentHoveredTactic = tactic;
            pendingHoverTactic = null;

            if (TooltipUIManager.Instance != null)
            {
                TooltipUIManager.Instance.ShowCustom(
                    tactic.tacticName,
                    tactic.GetDescription(),
                    mouse.position.ReadValue(),
                    tactic.GetCooldown()
                );
            }
        }
    }

    void CancelHover()
    {
        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        hoverRoutine = null;
        pendingHoverTactic = null;

        if (currentHoveredTactic != null)
        {
            if (TooltipUIManager.Instance != null) TooltipUIManager.Instance.Hide();
            currentHoveredTactic = null;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = mouse.position.ReadValue();
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    public void ShowTooltipFromUI(string tacticName, string description, float cooldown, Vector2 displayPosition)
    {
        CancelHover();
        isUIHoverDriven = true;

        if (TooltipUIManager.Instance != null)
        {
            TooltipUIManager.Instance.ShowCustom(
                tacticName,
                description,
                displayPosition,
                cooldown
            );
            TooltipUIManager.Instance.UpdatePosition(displayPosition);
        }
    }

    public void HideTooltipFromUI()
    {
        isUIHoverDriven = false;
        if (TooltipUIManager.Instance != null) TooltipUIManager.Instance.Hide();

        CancelHover();
    }
}
