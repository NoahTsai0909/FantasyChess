using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class UnitUIDebug : MonoBehaviour
{
    [Header("Debug Controls")]
    public bool logOnStart = true;
    public bool logOnUpdate = false;
    public Key debugKey = Key.F12;  // Using Input System Key enum

    private Canvas parentCanvas;
    private CanvasScaler canvasScaler;
    private RectTransform myRect;
    private VerticalLayoutGroup vlg;

    void Start()
    {
        // Get all components
        parentCanvas = GetComponentInParent<Canvas>();
        canvasScaler = parentCanvas?.GetComponent<CanvasScaler>();
        myRect = GetComponent<RectTransform>();
        vlg = GetComponent<VerticalLayoutGroup>();

        if (logOnStart)
            LogCanvasInfo();
    }

    void Update()
    {
        // Check for debug key using Input System
        if (Keyboard.current != null)
        {
            // Check if our specific key was pressed this frame
            KeyControl keyControl = Keyboard.current[debugKey];
            if (keyControl != null && keyControl.wasPressedThisFrame)
            {
                LogCanvasInfo();
            }
        }

        // Optional continuous logging (be careful with this!)
        if (logOnUpdate)
        {
            // Only log every 60 frames to avoid spam
            if (Time.frameCount % 60 == 0)
                LogCanvasInfo();
        }
    }

    // Also allow right-click menu in Inspector
    [ContextMenu("Log Canvas Info")]
    public void LogCanvasInfo()
    {
        Debug.Log("=====================================");
        Debug.Log($"=== UNIT UI DEBUG INFO at {System.DateTime.Now:T} ===");
        Debug.Log($"Frame: {Time.frameCount}");

        // Parent Canvas info
        if (parentCanvas != null)
        {
            Debug.Log($"<color=cyan>PARENT CANVAS:</color>");
            Debug.Log($"  - Name: {parentCanvas.name}");
            Debug.Log($"  - Render Mode: {parentCanvas.renderMode}");
            Debug.Log($"  - Scale Factor: {parentCanvas.scaleFactor}");
            Debug.Log($"  - Pixel Rect: {parentCanvas.pixelRect}");

            if (canvasScaler != null)
            {
                Debug.Log($"<color=cyan>CANVAS SCALER:</color>");
                Debug.Log($"  - UI Scale Mode: {canvasScaler.uiScaleMode}");
                Debug.Log($"  - Reference Resolution: {canvasScaler.referenceResolution}");
                Debug.Log($"  - Screen Match Mode: {canvasScaler.screenMatchMode}");
                Debug.Log($"  - Match Value: {canvasScaler.matchWidthOrHeight}");
                if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
                    Debug.Log($"  - Scale Factor: {canvasScaler.scaleFactor}");
            }
        }
        else
        {
            Debug.LogError("NO PARENT CANVAS FOUND!");
            // Try to find any canvas in scene
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Debug.Log($"Total Canvases in scene: {canvases.Length}");
            foreach (Canvas c in canvases)
            {
                Debug.Log($"  - Canvas: {c.name}, Render Mode: {c.renderMode}, Sort Order: {c.sortingOrder}");
            }
        }

        // My RectTransform info
        Debug.Log($"<color=yellow>MY RECTTRANSFORM:</color>");
        Debug.Log($"  - Size: {myRect.rect.size}");
        Debug.Log($"  - Anchors: min={myRect.anchorMin}, max={myRect.anchorMax}");
        Debug.Log($"  - Pivot: {myRect.pivot}");
        Debug.Log($"  - Position: {myRect.position}");
        Debug.Log($"  - Local Position: {myRect.localPosition}");

        // Layout Group info
        if (vlg != null)
        {
            Debug.Log($"<color=green>VERTICAL LAYOUT GROUP:</color>");
            Debug.Log($"  - Padding: L={vlg.padding.left}, R={vlg.padding.right}, T={vlg.padding.top}, B={vlg.padding.bottom}");
            Debug.Log($"  - Spacing: {vlg.spacing}");
            Debug.Log($"  - ChildAlignment: {vlg.childAlignment}");
            Debug.Log($"  - ChildControl Width: {vlg.childControlWidth}, Height: {vlg.childControlHeight}");
            Debug.Log($"  - ChildForceExpand Width: {vlg.childForceExpandWidth}, Height: {vlg.childForceExpandHeight}");
        }

        // Screen info
        Debug.Log($"<color=magenta>SCREEN INFO:</color>");
        Debug.Log($"  - Size: {Screen.width} x {Screen.height}");
        Debug.Log($"  - DPI: {Screen.dpi}");
        Debug.Log($"  - Fullscreen: {Screen.fullScreen}");
        Debug.Log($"  - Current Resolution: {Screen.currentResolution}");

        LogChildSizes();
        CheckForProblems();
    }

    void LogChildSizes()
    {
        Debug.Log($"<color=orange>CHILDREN ({transform.childCount} total):</color>");
        int index = 0;
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            LayoutElement layoutElem = child.GetComponent<LayoutElement>();
            LayoutGroup childLayout = child.GetComponent<LayoutGroup>();

            if (childRect != null)
            {
                Debug.Log($"Child {index}: {child.name}");
                Debug.Log($"Size: {childRect.rect.size}");
                Debug.Log($"Anchors: {childRect.anchorMin}, {childRect.anchorMax}");

                if (layoutElem != null)
                {
                    string layoutInfo = $"MinH:{layoutElem.minHeight} PrefH:{layoutElem.preferredHeight} FlexH:{layoutElem.flexibleHeight}";
                    if (layoutElem.preferredWidth > 0 || layoutElem.minWidth > 0)
                        layoutInfo += $" | MinW:{layoutElem.minWidth} PrefW:{layoutElem.preferredWidth} FlexW:{layoutElem.flexibleWidth}";
                    Debug.Log($"LayoutElement: {layoutInfo}");
                }

                if (childLayout != null)
                {
                    Debug.Log($"Has LayoutGroup: {childLayout.GetType().Name}");
                }

                // Check if this child is being ignored by layout
                if (layoutElem != null && layoutElem.ignoreLayout)
                    Debug.Log($"IGNORE LAYOUT is TRUE");
            }
            index++;
        }
    }

    void CheckForProblems()
    {
        Debug.Log($"<color=red>POTENTIAL ISSUES:</color>");
        bool hasIssues = false;

        // Check 1: Is this the right size?
        float expectedWidth = 420; // From your prefab
        if (Mathf.Abs(myRect.rect.width - expectedWidth) > 10)
        {
            Debug.Log($"Width ({myRect.rect.width:F1}) differs from prefab width ({expectedWidth})");
            hasIssues = true;
        }

        // Check 2: Are any children being squished?
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect != null && childRect.rect.height < 5 && child.gameObject.activeInHierarchy)
            {
                Debug.Log($" Child '{child.name}' has very small height: {childRect.rect.height:F1}");
                hasIssues = true;
            }
        }

        // Check 3: Layout conflicts
        if (vlg != null)
        {
            if (vlg.childControlWidth && vlg.childForceExpandWidth)
            {
                // This is usually fine, but check if any children have preferredWidth set
                foreach (Transform child in transform)
                {
                    LayoutElement le = child.GetComponent<LayoutElement>();
                    if (le != null && le.preferredWidth > 0)
                    {
                        Debug.Log($"Child '{child.name}' has PreferredWidth={le.preferredWidth} but parent forces width control. This setting will be ignored.");
                        hasIssues = true;
                    }
                }
            }
        }

        if (!hasIssues)
        {
            Debug.Log($"No obvious issues detected");
        }
    }

    // Visual debug in Scene view
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw outer bounds
        Vector3[] corners = new Vector3[4];
        myRect.GetWorldCorners(corners);

        Gizmos.color = Color.green;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }

        // Draw padding area if layout group exists
        if (vlg != null && vlg.padding != null)
        {
            // Calculate inner rect with padding
            Vector3 padding = new Vector3(vlg.padding.left, vlg.padding.bottom, 0);
            Gizmos.color = new Color(1, 1, 0, 0.3f); // Semi-transparent yellow

            // This is approximate - draws a smaller rect inside
            Vector3 center = myRect.position;
            Vector3 size = myRect.rect.size;
            size.x -= (vlg.padding.left + vlg.padding.right);
            size.y -= (vlg.padding.top + vlg.padding.bottom);

            Gizmos.DrawWireCube(center, size);
        }
    }
}