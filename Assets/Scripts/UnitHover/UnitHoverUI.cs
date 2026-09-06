using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UnitHoverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI provisionText;
    [SerializeField] private TextMeshProUGUI valueText;

    [SerializeField] private bool useFixedPosition = true;

    [SerializeField] private Sprite backgroundCommon;
    [SerializeField] private Sprite backgroundUncommon;
    [SerializeField] private Sprite backgroundRare;
    [SerializeField] private Sprite backgroundEpic;

    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private GameObject multicastContainer;
    [SerializeField] private TextMeshProUGUI multicastText;

    [SerializeField] private HealthBarUI healthBar;
    [SerializeField] private CooldownBarUI cooldownBar;
    [SerializeField] private Image backgroundImage;

    [Header("Ability UI")]
    [SerializeField] private GameObject activeAbilityBox;
    [SerializeField] private TextMeshProUGUI activeAbilityText;

    [SerializeField] private GameObject passiveAbilityBox;
    [SerializeField] private TextMeshProUGUI passiveAbilityText;

    [Header("Mutation UI")]
    [SerializeField] private GameObject mutationDivider;
    [SerializeField] private GameObject mutationAbilityBox;
    [SerializeField] private TextMeshProUGUI mutationAbilityText;

    [Header("Tags")]
    [SerializeField] private Transform tagContainer;
    [SerializeField] private GameObject tagBadgePrefab;

    [Header("Behavior")]
    [SerializeField] private bool isPermanentUI = false;
    [Tooltip("Extra padding to prevent IgnoreLayout elements from getting cut off at screen edges!")]
    [SerializeField] private Vector2 edgePadding = new Vector2(50f, 150f);

    [Header("Preview State")]
    public bool isPreviewMode = false;
    private UnitHoverUI activePreviewUI;
    [Tooltip("Drag the Inspect Notice and your new Preview Button here so they vanish on the cloned UI")]
    [SerializeField] private GameObject[] hideInPreviewMode;

    private int lastEnergy, lastAttack, lastShield, lastHeal, lastPoison, lastBurn, lastCrit, lastMulticast;

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
        if (isPreviewMode) return;

        if (currentUnit == null)
        {
            Hide();
            return;
        }

        UpdateDynamicValues();
        UpdateDynamicStats();
        UpdatePosition();

        if (activePreviewUI != null) activePreviewUI.UpdatePreviewPosition(this);
    }

    public void Show(UnitInstance unit)
    {
        if (unit == null || unit.Definition == null)
            return;

        currentUnit = unit;
        unit.RecalculateStats();

        string finalName = unit.Definition.unitName;
        string mutationColorHex = null;
        if (unit.currentPrefix != null)
        {
            mutationColorHex = ColorUtility.ToHtmlStringRGB(unit.currentPrefix.runeColor);
            finalName = $"<color=#{mutationColorHex}>{unit.currentPrefix.prefixName}</color> {finalName}";
        }

        if (unit.currentSuffix != null)
        {
            if (mutationColorHex != null)
            {
                finalName = $"{finalName} of <color=#{mutationColorHex}>{unit.currentSuffix.suffixName}</color>";
            }
            else
            {
                finalName = $"{finalName} of {unit.currentSuffix.suffixName}";
            }
        }

        nameText.text = finalName;

        SetRarityBackground(unit.CurrentRarity);
        cooldownBar.SetVisuals(unit.CurrentRarity);

        StatBlock stats = unit.Stats;

        string allStats = "";
        int displayEnergy = unit.inCombat ? unit.currentEnergy : stats.maxEnergy;

        if (unit.Definition.isEnergy) allStats += TextIconUtility.FormatEnergy(displayEnergy) + "/" + stats.maxEnergy + "  ";
        if (unit.Stats.Tags.HasFlag(UnitTagFlags.Damage) && stats.Attack > 0) allStats += TextIconUtility.FormatAttack(stats.Attack) + "  ";
        if (unit.Stats.Tags.HasFlag(UnitTagFlags.Shield) && stats.Shield > 0) allStats += TextIconUtility.FormatShield(stats.Shield) + "  ";
        if (unit.Stats.Tags.HasFlag(UnitTagFlags.Heal) && stats.Heal > 0) allStats += TextIconUtility.FormatHeal(stats.Heal) + "  ";
        if (unit.Stats.Tags.HasFlag(UnitTagFlags.Poison) && stats.Poison > 0) allStats += TextIconUtility.FormatPoison(stats.Poison) + "  ";
        if (unit.Stats.Tags.HasFlag(UnitTagFlags.Burn) && stats.Burn > 0) allStats += TextIconUtility.FormatBurn(stats.Burn) + "  ";
        if (unit.GetActiveDescription() != "" && stats.CritChance > 0) allStats += TextIconUtility.FormatCrit(stats.CritChance) + "  ";
        statText.SetText(allStats);

        if (unit.Definition.isPassive)
        {
            cooldownBar.gameObject.SetActive(false);
        }
        else
        {
            cooldownBar.gameObject.SetActive(true);
        }

        provisionText.SetText(TextIconUtility.FormatProvision(unit.Definition.provisionCost));
        valueText.SetText(TextIconUtility.FormatGold(stats.Value));
        multicastContainer.SetActive(stats.Multicast > 1);
        multicastText.SetText(TextIconUtility.FormatMulticast(stats.Multicast));

        lastEnergy = -1;

        UpdateDynamicStats();
        UpdateDynamicValues();
        // Show and position
        gameObject.SetActive(true);
        if (!useFixedPosition)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            UpdatePosition();
        }
        else
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
        UpdateAbilityDescriptions();
        foreach (Transform child in tagContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // 2. Safely get the unit's tags
        UnitTagFlags unitTags = unit.Stats.Tags;

        // 3. Loop through every possible tag defined in your UnitTagFlags enum
        foreach (UnitTagFlags flag in System.Enum.GetValues(typeof(UnitTagFlags)))
        {
            if (flag == UnitTagFlags.None) continue;

            if (flag == UnitTagFlags.Damage || flag == UnitTagFlags.Shield || flag == UnitTagFlags.Heal || flag == UnitTagFlags.Poison || flag == UnitTagFlags.Burn || flag == UnitTagFlags.Crit || flag == UnitTagFlags.Energy || flag == UnitTagFlags.Slow || flag == UnitTagFlags.MaxHP || flag == UnitTagFlags.Haste)
            {
                continue;
            }

            if (flag == UnitTagFlags.BurnRef || flag == UnitTagFlags.PoisonRef || flag == UnitTagFlags.DamageRef || flag == UnitTagFlags.HealRef)
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
        if (activePreviewUI != null) Destroy(activePreviewUI.gameObject);

        if (isPermanentUI) return;
        currentUnit = null;
        gameObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (canvas == null || currentUnit == null || mainCamera == null) return;
        
        if (useFixedPosition)
        {
            Vector2 screenPos = mainCamera.WorldToScreenPoint(currentUnit.transform.position);

            float flipThreshold = Screen.width * 0.7f;

            bool unitIsOnLeft = screenPos.x < flipThreshold;

            float edgePadding = 50f;

            if (unitIsOnLeft)
            {
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.pivot = new Vector2(1, 0.5f);
                rectTransform.anchoredPosition = new Vector2(-edgePadding, 0f);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
                rectTransform.pivot = new Vector2(0, 0.5f);
                rectTransform.anchoredPosition = new Vector2(edgePadding, 0f);
            }

            return; // Exit early so we don't run the relative positioning code below
        }
        Vector3 unitWorldPos = currentUnit.transform.position;

        Collider2D collider = currentUnit.GetComponent<Collider2D>();
        float unitHeight = 2f; 

        if (collider != null)
        {
            unitHeight = collider.bounds.size.y;
        }

        float uiWidth = (rectTransform.rect.width + edgePadding.x) * canvas.scaleFactor;
        float uiHeight = (rectTransform.rect.height + edgePadding.y) * canvas.scaleFactor;


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

    private void UpdateDynamicStats()
    {
        StatBlock stats = currentUnit.Stats;
        int currentEnergy = currentUnit.inCombat ? currentUnit.currentEnergy : stats.maxEnergy;

        // 1. PERFORMANCE CHECK: Did anything actually change?
        if (lastEnergy == currentEnergy &&
            lastAttack == stats.Attack &&
            lastShield == stats.Shield &&
            lastHeal == stats.Heal &&
            lastPoison == stats.Poison &&
            lastBurn == stats.Burn &&
            lastCrit == stats.CritChance &&
            lastMulticast == stats.Multicast)
        {
            return; // Nothing changed, skip expensive string rebuilding!
        }

        // 2. UPDATE CACHE
        lastEnergy = currentEnergy;
        lastAttack = stats.Attack;
        lastShield = stats.Shield;
        lastHeal = stats.Heal;
        lastPoison = stats.Poison;
        lastBurn = stats.Burn;
        lastCrit = stats.CritChance;
        lastMulticast = stats.Multicast;

        // 3. REBUILD STRING (Only runs when a stat fluctuates)
        string allStats = "";

        if (currentUnit.Definition.isEnergy) allStats += TextIconUtility.FormatEnergy(currentEnergy) + "/" + stats.maxEnergy + "  ";
        if (currentUnit.Stats.Tags.HasFlag(UnitTagFlags.Damage) && stats.Attack > 0) allStats += TextIconUtility.FormatAttack(stats.Attack) + "  ";
        if (currentUnit.Stats.Tags.HasFlag(UnitTagFlags.Shield) && stats.Shield > 0) allStats += TextIconUtility.FormatShield(stats.Shield) + "  ";
        if (currentUnit.Stats.Tags.HasFlag(UnitTagFlags.Heal) && stats.Heal > 0) allStats += TextIconUtility.FormatHeal(stats.Heal) + "  ";
        if (currentUnit.Stats.Tags.HasFlag(UnitTagFlags.Poison) && stats.Poison > 0) allStats += TextIconUtility.FormatPoison(stats.Poison) + "  ";
        if (currentUnit.Stats.Tags.HasFlag(UnitTagFlags.Burn) && stats.Burn > 0) allStats += TextIconUtility.FormatBurn(stats.Burn) + "  ";
        if (currentUnit.GetActiveDescription() != "" && stats.CritChance > 0) allStats += TextIconUtility.FormatCrit(stats.CritChance) + "  ";
        if (stats.Multicast > 1) multicastText.SetText(TextIconUtility.FormatMulticast(stats.Multicast)); else multicastContainer.SetActive(false);

        statText.SetText(allStats);
        UpdateAbilityDescriptions();
    }

    public void ToggleUpgradePreview()
    {
        if (activePreviewUI != null)
        {
            Destroy(activePreviewUI.gameObject);
            return;
        }

        if (!currentUnit.CanUpgradeTier()) return;

        // 1. Spawn a perfect clone of this UI
        activePreviewUI = Instantiate(this, transform.parent);
        activePreviewUI.isPreviewMode = true;

        foreach (GameObject obj in activePreviewUI.hideInPreviewMode)
        {
            if (obj != null) obj.SetActive(false);
        }

        Rarity originalRarity = currentUnit.CurrentRarity;
        currentUnit.PreviewRaritySwap(RarityScaling.GetNextRarity(originalRarity));

        activePreviewUI.Show(currentUnit);

        currentUnit.PreviewRaritySwap(originalRarity);

        activePreviewUI.UpdatePreviewPosition(this);
    }

    public void UpdatePreviewPosition(UnitHoverUI parentUI)
    {
        float offset = parentUI.rectTransform.rect.width + 20f;

        if (parentUI.useFixedPosition)
        {
            if (parentUI.rectTransform.anchorMin.x == 1)
                rectTransform.anchoredPosition = parentUI.rectTransform.anchoredPosition + new Vector2(-offset, 0);
            else
                rectTransform.anchoredPosition = parentUI.rectTransform.anchoredPosition + new Vector2(offset, 0);
        }
        else
        {
            if (parentUI.rectTransform.position.x > Screen.width / 2f)
                rectTransform.anchoredPosition = parentUI.rectTransform.anchoredPosition + new Vector2(-offset, 0);
            else
                rectTransform.anchoredPosition = parentUI.rectTransform.anchoredPosition + new Vector2(offset, 0);
        }
    }

    private void UpdateAbilityDescriptions()
    {
        string activeDesc = TextIconUtility.ParseDescription(currentUnit.GetActiveDescription());
        string passiveDesc = TextIconUtility.ParseDescription(currentUnit.GetPassiveDescription());

        if (currentUnit.currentSuffix != null)
        {
            string triggerText = currentUnit.GetMutationTriggerText();
            string actionPhrase = currentUnit.currentSuffix.GetActionPhrase(currentUnit, true);

            string suffixLabel = currentUnit.currentSuffix.suffixName;
            if (currentUnit.currentPrefix != null)
            {
                string hex = ColorUtility.ToHtmlStringRGB(currentUnit.currentPrefix.runeColor);
                suffixLabel = $"<color=#{hex}>{suffixLabel}</color>";
            }

            string mutationSentence = $"<br>{triggerText}{suffixLabel}: {actionPhrase}.";
            mutationSentence = TextIconUtility.ParseDescription(mutationSentence);

            if (currentUnit.Definition.isPassive)
                passiveDesc += mutationSentence;
            else
                activeDesc += mutationSentence;
        }

        if (!string.IsNullOrEmpty(activeDesc))
        {
            activeAbilityBox.SetActive(true);
            activeAbilityText.SetText(activeDesc);
        }
        else
        {
            activeAbilityBox.SetActive(false);
        }

        if (!string.IsNullOrEmpty(passiveDesc))
        {
            passiveAbilityBox.SetActive(true);
            passiveAbilityText.SetText(passiveDesc);
        }
        else
        {
            passiveAbilityBox.SetActive(false);
        }

        string mutationScalingDesc = currentUnit.GetMutationScalingText();
        if (!string.IsNullOrEmpty(mutationScalingDesc))
        {
            if (mutationDivider != null) mutationDivider.SetActive(true);
            if (mutationAbilityBox != null) mutationAbilityBox.SetActive(true);

            if (mutationAbilityText != null)
                mutationAbilityText.SetText(TextIconUtility.ParseDescription(mutationScalingDesc));
        }
        else
        {
            if (mutationDivider != null) mutationDivider.SetActive(false);
            if (mutationAbilityBox != null) mutationAbilityBox.SetActive(false);
        }
    }


}