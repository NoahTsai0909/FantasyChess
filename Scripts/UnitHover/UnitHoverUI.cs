using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UnitHoverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI provisionText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Vector2 offset = new Vector2(20f, 20f);

    [SerializeField] private bool useFixedPosition = true;

    [SerializeField] private Sprite backgroundCommon;
    [SerializeField] private Sprite backgroundUncommon;
    [SerializeField] private Sprite backgroundRare;
    [SerializeField] private Sprite backgroundEpic;

    [SerializeField] private TextMeshProUGUI statText;

    [SerializeField] private HealthBarUI healthBar;
    [SerializeField] private CooldownBarUI cooldownBar;
    [SerializeField] private Image backgroundImage;

    [Header("Ability UI")]
    [SerializeField] private GameObject activeAbilityBox;
    [SerializeField] private TextMeshProUGUI activeAbilityText;

    [SerializeField] private GameObject passiveAbilityBox;
    [SerializeField] private TextMeshProUGUI passiveAbilityText;

    [Header("Tags")]
    [SerializeField] private Transform tagContainer;
    [SerializeField] private GameObject tagBadgePrefab;

    [Header("Behavior")]
    [SerializeField] private bool isPermanentUI = false;


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

        //gameObject.SetActive(false);
        if (!isPermanentUI) gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentUnit == null)
            return;

        UpdateDynamicValues();
        UpdatePosition();
    }

    public void Show(UnitInstance unit)
    {
        if (unit == null || unit.Definition == null)
            return;

        currentUnit = unit;
        unit.RecalculateStats();

        // Set UI content
        nameText.text = unit.Definition.unitName;
        SetRarityBackground(unit.CurrentRarity);
        cooldownBar.SetVisuals(unit.CurrentRarity);

        StatBlock stats = unit.Stats;

        string allStats = "";
        int displayEnergy = unit.inCombat ? unit.currentEnergy : stats.maxEnergy;
        if (unit.Definition.isEnergy) allStats += TextIconUtility.FormatEnergy(displayEnergy) + "/" + stats.maxEnergy + "  ";
        if (unit.Definition.tagFlags.HasFlag(UnitTagFlags.Damage) && stats.Attack > 0) allStats += TextIconUtility.FormatAttack(stats.Attack) + "  ";
        if (unit.Definition.tagFlags.HasFlag(UnitTagFlags.Shield) && stats.Shield > 0) allStats += TextIconUtility.FormatShield(stats.Shield) + "  ";
        if (unit.Definition.tagFlags.HasFlag(UnitTagFlags.Heal) && stats.Heal > 0) allStats += TextIconUtility.FormatHeal(stats.Heal) + "  ";
        if (unit.Definition.tagFlags.HasFlag(UnitTagFlags.Poison) && stats.Poison > 0) allStats += TextIconUtility.FormatPoison(stats.Poison) + "  ";
        if (unit.Definition.tagFlags.HasFlag(UnitTagFlags.Burn) && stats.Burn > 0) allStats += TextIconUtility.FormatBurn(stats.Burn) + "  ";
        if (unit.Definition.tagFlags.HasFlag(UnitTagFlags.Crit) && stats.CritChance > 0) allStats += TextIconUtility.FormatCrit(stats.CritChance) + "  ";
        statText.SetText(allStats);

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

        string activeDesc = TextIconUtility.ParseDescription(unit.GetActiveDescription());
        string passiveDesc = TextIconUtility.ParseDescription(unit.GetPassiveDescription());

        // Handle Active Ability Box
        if (!string.IsNullOrEmpty(activeDesc))
        {
            activeAbilityBox.SetActive(true);
            activeAbilityText.SetText(activeDesc);
        }
        else
        {
            activeAbilityBox.SetActive(false);
        }

        // Handle Passive Ability Box
        if (!string.IsNullOrEmpty(passiveDesc))
        {
            passiveAbilityBox.SetActive(true);
            passiveAbilityText.SetText(passiveDesc);
        }
        else
        {
            passiveAbilityBox.SetActive(false);
        }

        foreach (Transform child in tagContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Safely get the unit's tags
        UnitTagFlags unitTags = unit.Definition.tagFlags;

        // 3. Loop through every possible tag defined in your UnitTagFlags enum
        foreach (UnitTagFlags flag in System.Enum.GetValues(typeof(UnitTagFlags)))
        {
            if (flag == UnitTagFlags.None) continue;

            if (flag == UnitTagFlags.Damage || flag == UnitTagFlags.Shield || flag == UnitTagFlags.Heal || flag == UnitTagFlags.Poison || flag == UnitTagFlags.Burn || flag == UnitTagFlags.Crit || flag == UnitTagFlags.Energy || flag == UnitTagFlags.Slow || flag == UnitTagFlags.MaxHP)
            {
                continue;
            }

            // 4. Check if the unit actually has this specific flag
            if (unitTags.HasFlag(flag))
            {
                // Spawn the dark background prefab into the container
                GameObject newBadge = Instantiate(tagBadgePrefab, tagContainer);

                // Find the TextMeshPro child inside the prefab and set the word
                TextMeshProUGUI badgeText = newBadge.GetComponentInChildren<TextMeshProUGUI>();
                if (badgeText != null)
                {
                    badgeText.text = flag.ToString();
                }
            }
        }
    }

    public void Hide()
    {
        if (isPermanentUI) return;
        currentUnit = null;
        gameObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (canvas == null || currentUnit == null || mainCamera == null) return;

        if(useFixedPosition)
        {
            // ADAPTIVE FIXED POSITIONING
            // Find out if the unit is on the left or right side of the screen
            Vector2 screenPos = mainCamera.WorldToScreenPoint(currentUnit.transform.position);

            // THE FIX: Move the threshold line from 50% to 70%
            float flipThreshold = Screen.width * 0.7f;

            // If the unit is anywhere in the left 70% of the screen, spawn UI on the Right.
            bool unitIsOnLeft = screenPos.x < flipThreshold;

            // Give it a little padding from the edge of the screen
            float edgePadding = 50f;

            if (unitIsOnLeft)
            {
                // Snap UI to the RIGHT side of the screen
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.pivot = new Vector2(1, 0.5f);
                rectTransform.anchoredPosition = new Vector2(-edgePadding, 0f);
            }
            else
            {
                // Snap UI to the LEFT side of the screen
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
                rectTransform.pivot = new Vector2(0, 0.5f);
                rectTransform.anchoredPosition = new Vector2(edgePadding, 0f);
            }

            return; // Exit early so we don't run the relative positioning code below
        }
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