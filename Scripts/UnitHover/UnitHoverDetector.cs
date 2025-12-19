using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UnitHoverDetector : MonoBehaviour
{
    [SerializeField] private UnitHoverUI hoverUI;
    [SerializeField] private float hoverDelay = 0.35f;

    private Camera mainCamera;
    private Mouse mouse;

    private UnitInstance currentHoveredUnit;
    private UnitInstance pendingHoverUnit;
    private Coroutine hoverRoutine;

    void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
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
            hoverUI.Show(unit);
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
            hoverUI.Hide();
            currentHoveredUnit = null;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = mouse.position.ReadValue();
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
}
