using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipUIManager : MonoBehaviour
{
    public static TooltipUIManager Instance { get; private set; }

    [Header("Database")]
    [SerializeField] private TooltipDatabaseSO database;

    [Header("UI References")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cooldownUI;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(20f, 20f);

    private RectTransform canvasRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Hide();
    }

    public void Show(string id, Vector2 mousePosition)
    {
        if (database == null) return;

        TooltipEntry entry = database.GetEntry(id);
        if (entry == null) return;

        titleText.text = entry.title;
        descriptionText.text = entry.description;

        // Force Unity to immediately resize the panel to fit the new text
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);

        UpdatePosition(mousePosition);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowCustom(string title, string description, Vector2 mousePosition, float cooldown = 0f)
    {
        titleText.text = title;
        descriptionText.SetText(TextIconUtility.ParseDescription(description));
        if (cooldown > 0f)
        {
            cooldownUI.gameObject.SetActive(true);
            cooldownText.text = $"{cooldown:F1}";
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);

        UpdatePosition(mousePosition);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        cooldownUI.gameObject.SetActive(false);
    }

    public void UpdatePosition(Vector2 mousePosition)
    {
        if (canvasGroup.alpha == 0f) return;

        // Convert the screen mouse position to local canvas coordinates
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mousePosition,
            null, // Use null if the Canvas is Screen Space - Overlay
            out Vector2 localPoint);

        // Apply the offset so it doesn't cover the cursor
        localPoint += cursorOffset;

        // Clamp to screen edges to prevent the tooltip from going off-screen
        float pivotX = localPoint.x + tooltipPanel.rect.width > canvasRect.rect.width / 2 ? 1f : 0f;
        float pivotY = localPoint.y - tooltipPanel.rect.height < -canvasRect.rect.height / 2 ? 0f : 1f;
        tooltipPanel.pivot = new Vector2(pivotX, pivotY);

        tooltipPanel.anchoredPosition = localPoint;
    }
}
