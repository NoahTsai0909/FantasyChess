using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UnitHoverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private TextMeshProUGUI provisionText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Vector2 offset = new Vector2(20f, 20f);

    [SerializeField] private bool useFixedPosition = true;

    [SerializeField] private Sprite backgroundCommon;
    [SerializeField] private Sprite backgroundUncommon;
    [SerializeField] private Sprite backgroundRare;
    [SerializeField] private Sprite backgroundEpic;

    [SerializeField] private StatWidget statWidgetPrefab;
    [SerializeField] private Transform statsContainer;

    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite shieldIcon;
    [SerializeField] private Sprite healIcon;
    [SerializeField] private Sprite poisonIcon;
    [SerializeField] private Sprite burnIcon;

    [SerializeField] private HealthBarUI healthBar;
    [SerializeField] private CooldownBarUI cooldownBar;
    [SerializeField] private Image backgroundImage;

    private Canvas canvas;
    private RectTransform rectTransform;
    private UnitInstance currentUnit;
    private Camera mainCamera;
    private Camera canvasCamera;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;

        // Determine which camera to use for screen-to-canvas conversion
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = null; // Overlay canvases don't use a camera
        }
        else
        {
            canvasCamera = canvas.worldCamera ?? mainCamera;
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentUnit == null)
            return;

        UpdateDynamicValues();
        if (!useFixedPosition)
        {
            UpdatePosition();
        }
    }

    public void Show(UnitInstance unit)
    {
        if (unit == null || unit.Definition == null)
            return;

        currentUnit = unit;
        unit.RecalculateStats();

        // Set UI content
        nameText.text = unit.Definition.unitName;
        abilityText.text = unit.GetAbilityDescription();
        SetRarityBackground(unit.CurrentRarity);

        // Clear and repopulate stat widgets
        foreach (Transform child in statsContainer)
            Destroy(child.gameObject);

        StatBlock stats = unit.Stats;
        if (stats.Attack > 0) AddStatWidget(attackIcon, stats.Attack);
        if (stats.Shield > 0) AddStatWidget(shieldIcon, stats.Shield);
        if (stats.Heal > 0) AddStatWidget(healIcon, stats.Heal);
        if (stats.Poison > 0) AddStatWidget(poisonIcon, stats.Poison);
        if (stats.Burn > 0) AddStatWidget(burnIcon, stats.Burn);

        if (unit.Definition.isPassive)
        {
            cooldownBar.gameObject.SetActive(false);
        }
        else
        {
            cooldownBar.gameObject.SetActive(true);
        }

        provisionText.text = unit.Definition.provisionCost.ToString();
        valueText.text = stats.Value.ToString();

        // Show and position
        gameObject.SetActive(true);
        if (!useFixedPosition)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            UpdatePosition();
        }
        else
        {
            // Just rebuild layout to ensure content fits, but keep position
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            // Ensure our anchor position is maintained
            if (canvas != null && rectTransform != null)
            {
                // Re-apply the anchored position to be safe
                // You could also just skip this if the position stays
            }
        }
    }

    public void Hide()
    {
        currentUnit = null;
        gameObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (canvas == null || currentUnit == null || mainCamera == null) return;

        // Get the unit's world position
        Vector3 unitWorldPos = currentUnit.transform.position;

        // Get the unit's collider to find its top and bottom
        Collider2D collider = currentUnit.GetComponent<Collider2D>();
        float unitHeight = 2f; // default fallback

        if (collider != null)
        {
            unitHeight = collider.bounds.size.y;
        }

        // Calculate UI size
        float uiWidth = rectTransform.rect.width * canvas.scaleFactor;
        float uiHeight = rectTransform.rect.height * canvas.scaleFactor;

        // First, try to position ABOVE the unit
        Vector3 aboveWorldPos = unitWorldPos;
        if (collider != null)
        {
            aboveWorldPos.y = collider.bounds.max.y + 5.0f; // Your preferred padding
        }
        else
        {
            aboveWorldPos.y += unitHeight * 0.5f + 5.0f;
        }

        Vector2 aboveScreenPos = mainCamera.WorldToScreenPoint(aboveWorldPos);

        // Check if there's space above (UI won't go off-screen)
        bool hasSpaceAbove = aboveScreenPos.y + uiHeight * 0.5f < Screen.height;

        Vector2 targetScreenPos;

        if (hasSpaceAbove)
        {
            // Position above the unit
            targetScreenPos = aboveScreenPos;
        }
        else
        {
            // Position BELOW the unit
            Vector3 belowWorldPos = unitWorldPos;
            if (collider != null)
            {
                belowWorldPos.y = collider.bounds.min.y - 5.0f; // Below with padding
            }
            else
            {
                belowWorldPos.y -= unitHeight * 0.5f + 5.0f;
            }

            targetScreenPos = mainCamera.WorldToScreenPoint(belowWorldPos);
        }

        // Clamp horizontal position to keep UI on screen
        targetScreenPos.x = Mathf.Clamp(targetScreenPos.x, uiWidth * 0.5f, Screen.width - uiWidth * 0.5f);

        // Also clamp vertical just in case (though we already checked)
        targetScreenPos.y = Mathf.Clamp(targetScreenPos.y, uiHeight * 0.5f, Screen.height - uiHeight * 0.5f);

        // Convert to canvas space
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 anchoredPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, targetScreenPos, canvasCamera, out anchoredPos))
        {
            rectTransform.anchoredPosition = anchoredPos;
        }
    }

    private void SetRarityBackground(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: backgroundImage.sprite = backgroundCommon; break;
            case Rarity.Uncommon: backgroundImage.sprite = backgroundUncommon; break;
            case Rarity.Rare: backgroundImage.sprite = backgroundRare; break;
            case Rarity.Epic: backgroundImage.sprite = backgroundEpic; break;
        }
    }

    private void AddStatWidget(Sprite icon, int value)
    {
        StatWidget widget = Instantiate(statWidgetPrefab, statsContainer);
        widget.Set(icon, value);
    }

    private void UpdateDynamicValues()
    {
        if (currentUnit.inCombat)
        {
            healthBar.SetHoverUIValues(currentUnit.GetCurrentHP(), currentUnit.Stats.MaxHP, currentUnit.GetCurrentShield());
            healthBar.SetTextVisible(true);
            cooldownBar.SetValues(currentUnit.GetCooldownTimer(), currentUnit.Stats.Cooldown);
        }
        else
        {
            healthBar.SetHoverUIValues(currentUnit.Stats.MaxHP, currentUnit.Stats.MaxHP, currentUnit.GetCurrentShield());
            healthBar.SetTextVisible(true);
            cooldownBar.SetValues(currentUnit.Stats.Cooldown, currentUnit.Stats.Cooldown);
        }
    }
}