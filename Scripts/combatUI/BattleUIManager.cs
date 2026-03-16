using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private GameObject cooldownBarPrefab;
    [SerializeField] private Canvas battleCanvas; // Reference to your battle UI Canvas
    [SerializeField] private FloatingCombatText floatingTextPrefab;
    [SerializeField] private GameObject statusEffectBarPrefab;

    private Dictionary<UnitInstance, (HealthBarUI healthBar, CooldownBarUI cooldownBar, StatusEffectBar statusBar)> unitUIElements = new();


    private GridManager playerGrid;
    private GridManager enemyGrid;

    public void Initialize(GridManager playerGrid, GridManager enemyGrid)
    {
        this.playerGrid = playerGrid;
        this.enemyGrid = enemyGrid;

        if (battleCanvas == null)
        {
            battleCanvas = FindFirstObjectByType<Canvas>();
        }
    }

    public void CreateUnitUI(UnitInstance unit, Vector3 worldPosition)
    {
        if (healthBarPrefab == null || cooldownBarPrefab == null || battleCanvas == null)
        {
            Debug.LogWarning("Battle UI manager not properly initialized!");
            return;
        }

        Vector3 uiPosition = worldPosition;

        // Create health bar
        GameObject healthBarGO = Instantiate(healthBarPrefab, battleCanvas.transform);
        healthBarGO.transform.position = uiPosition + GetHealthBarOffset(unit.isPlayer);

        HealthBarUI healthBar = healthBarGO.GetComponent<HealthBarUI>();

        // Create cooldown bar
        GameObject cooldownBarGO = Instantiate(cooldownBarPrefab, battleCanvas.transform);
        cooldownBarGO.transform.position = uiPosition + GetCooldownBarOffset(unit.isPlayer);
        cooldownBarGO.transform.localScale = Vector3.one * 0.75f;

        CooldownBarUI cooldownBar = cooldownBarGO.GetComponent<CooldownBarUI>();

        healthBar.SetTextVisible(false);
        cooldownBar.SetTextVisible(false);

        // Status bar stays the same
        GameObject statusBarGO = Instantiate(statusEffectBarPrefab, battleCanvas.transform);
        statusBarGO.transform.position = uiPosition + GetStatusBarOffset(unit.isPlayer);

        StatusEffectBar statusBar = statusBarGO.GetComponent<StatusEffectBar>();

        // Store references
        unitUIElements[unit] = (healthBar, cooldownBar, statusBar);

        // Initialize UI values
        if (unit.inCombat)
        {
            healthBar.SetValues(unit.GetCurrentHP(), unit.Stats.MaxHP, unit.GetCurrentShield());
            cooldownBar.SetValues(unit.GetCooldownTimer(), unit.Stats.Cooldown);
        }
        else
        {
            healthBar.SetValues(unit.Stats.MaxHP, unit.Stats.MaxHP, 0);
            cooldownBar.SetValues(0, unit.Stats.Cooldown);
        }
    }

    public void UpdateUnitUI(UnitInstance unit, Vector3 worldPosition)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements))
        {
            Vector3 uiPosition = worldPosition;

            uiElements.healthBar.transform.position =
                uiPosition + GetHealthBarOffset(unit.isPlayer);

            uiElements.cooldownBar.transform.position =
                uiPosition + GetCooldownBarOffset(unit.isPlayer);

            uiElements.statusBar.transform.position =
                uiPosition + GetStatusBarOffset(unit.isPlayer);
        }
    }

    public void UpdateHealthBar(UnitInstance unit)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements))
        {
            uiElements.healthBar.SetValues(
                unit.GetCurrentHP(),
                unit.Stats.MaxHP,
                unit.GetCurrentShield()
            );
        }
    }

    public void UpdateCooldownBar(UnitInstance unit)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements))
        {
            uiElements.cooldownBar.SetValues(
                unit.GetCooldownTimer(),
                unit.Stats.Cooldown
            );
        }
    }

    public void RemoveUnitUI(UnitInstance unit)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements))
        {
            if (uiElements.healthBar != null)
                Destroy(uiElements.healthBar.gameObject);

            if (uiElements.cooldownBar != null)
                Destroy(uiElements.cooldownBar.gameObject);

            if (uiElements.statusBar != null)
                Destroy(uiElements.statusBar.gameObject);

            unitUIElements.Remove(unit);
        }
    }

    private Vector3 GetHealthBarOffset(bool isPlayer)
    {
        // Adjust Y offset based on sprite height - try larger values
        return new Vector3(0, -2.5f, 0); // Changed from -0.5f to -1f
    }

    private Vector3 GetCooldownBarOffset(bool isPlayer)
    {
        return new Vector3(-1.5f, -2.5f, 0); // Changed to be more visible
    }

    private Vector3 GetStatusBarOffset(bool isPlayer)
    {
        return new Vector3(0, -2f, 0);
    }

    private Image FindHealthBarImage(GameObject healthBar)
    {
        // Look for Canvas/Bar first (your prefab structure)
        Transform barTransform = healthBar.transform.Find("Canvas/Bar");
        if (barTransform == null)
        {
            // Alternative search
            barTransform = healthBar.transform.Find("Bar");
        }

        if (barTransform != null)
        {
            return barTransform.GetComponent<Image>();
        }
        else
        {
            // Fallback
            return healthBar.GetComponentInChildren<Image>();
        }
    }

    private Image FindCooldownBarImage(GameObject cooldownBar)
    {
        // Look for Canvas/CooldownBar first
        Transform barTransform = cooldownBar.transform.Find("Canvas/CooldownBar");
        if (barTransform == null)
        {
            barTransform = cooldownBar.transform.Find("CooldownBar");
        }

        if (barTransform != null)
        {
            return barTransform.GetComponent<Image>();
        }
        else
        {
            return cooldownBar.GetComponentInChildren<Image>();
        }
    }

    private Image FindShieldBarImage(GameObject healthBar)
    {
        // Look for Canvas/Bar first (your prefab structure)
        Transform barTransform = healthBar.transform.Find("Canvas/ShieldBar");
        if (barTransform == null)
        {
            // Alternative search
            barTransform = healthBar.transform.Find("ShieldBar");
        }

        if (barTransform != null)
        {
            return barTransform.GetComponent<Image>();
        }
        else
        {
            // Fallback
            return healthBar.GetComponentInChildren<Image>();
        }
    }

    private void OnEnable()
    {
        CombatEventBus.OnActionResolved += HandleActionResolved;
        CombatEventBus.OnStatusChanged += HandleStatusChanged;
    }

    private void OnDisable()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
        CombatEventBus.OnStatusChanged -= HandleStatusChanged;
    }

    private void HandleActionResolved(CombatAction action)
    {
        if (action.target == null)
            return;

        Vector3 worldPos = action.target.transform.position;

        switch (action.type)
        {
            case CombatActionType.Damage:
                SpawnFloatingText(worldPos, $"-{action.amount}", Color.red);
                break;

            case CombatActionType.Heal:
                SpawnFloatingText(worldPos, $"+{action.amount}", Color.green);
                break;

            case CombatActionType.Shield:
                SpawnFloatingText(worldPos, $"+{action.amount}", Color.gold);
                break;
            case CombatActionType.BurnTick:
                SpawnFloatingText(worldPos, $"-{action.amount}", Color.orange);
                break;
        }
    }

    private void SpawnFloatingText(Vector3 worldPos, string text, Color color)
    {
        var instance = Instantiate(
            floatingTextPrefab,
            worldPos + Vector3.up * 0.5f + Vector3.right,
            Quaternion.identity
        );

        instance.Initialize(text, color);
    }

    private void HandleStatusChanged(UnitInstance unit, StatusEffectType type, int stacks)
    {
        if (!unitUIElements.TryGetValue(unit, out var ui))
            return;

        ui.statusBar.SetStatus(type, stacks);
    }
}
