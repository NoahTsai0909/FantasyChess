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
    private Dictionary<UnitInstance, float> nextPopupTime = new();


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
        healthBarGO.transform.localScale = new Vector3(0.9f, 0.4f, 1f);

        HealthBarUI healthBar = healthBarGO.GetComponent<HealthBarUI>();

        // Create cooldown bar
        GameObject cooldownBarGO = Instantiate(cooldownBarPrefab, battleCanvas.transform);
        cooldownBarGO.transform.position = uiPosition + GetCooldownBarOffset(unit.isPlayer);
        cooldownBarGO.transform.localScale = Vector3.one * 1.0f;

        CooldownBarUI cooldownBar = cooldownBarGO.GetComponent<CooldownBarUI>();

        //healthBar.SetTextVisible(false);
        cooldownBar.SetTextVisible(false);
        cooldownBar.SetVisuals(unit.CurrentRarity);
        if (unit.Definition.isPassive)
        {
            cooldownBarGO.SetActive(false);
        }

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
        return new Vector3(0, -3.25f, 0);
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
        if (action.target == null || action.isSilent)
            return;

        float spawnTime = Time.time;

        if (nextPopupTime.TryGetValue(action.target, out float nextTime))
        {
            spawnTime = Mathf.Max(Time.time, nextTime);
        }

        nextPopupTime[action.target] = spawnTime + 0.15f;

        StartCoroutine(SpawnPopupDelayed(action, spawnTime - Time.time));

        Vector3 worldPos = action.target.transform.position;

    }

    private System.Collections.IEnumerator SpawnPopupDelayed(CombatAction action, float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        if (action.target == null)
            yield break;

        Vector3 worldPos = action.target.transform.position;

        switch (action.type)
        {
            case CombatActionType.Damage:
                SpawnFloatingText(worldPos, $"{action.amount}", Color.red, action.isCrit);
                break;

            case CombatActionType.Heal:
                SpawnFloatingText(worldPos, $"[HEAL]{action.amount}", Color.green, action.isCrit);
                break;

            case CombatActionType.Shield:
                SpawnFloatingText(worldPos, $"[SHIELD]{action.amount}", Color.gold, action.isCrit);
                break;

            case CombatActionType.BurnTick:
                SpawnFloatingText(worldPos, $"[BURN]{action.amount}", Color.orange, false);
                break;
        }
    }

    private void SpawnFloatingText(Vector3 worldPos, string text, Color color, bool isCrit)
    {
        Vector3 offset = new Vector3(Random.Range(-0.1f, 0.1f),1f + Random.Range(-0.2f, 0.2f),0f);
        var instance = Instantiate(
            floatingTextPrefab,
            worldPos + offset,
            Quaternion.identity
        );

        instance.Initialize(text, color, isCrit);
    }

    private void HandleStatusChanged(UnitInstance unit, StatusEffectType type, int stacks)
    {
        if (!unitUIElements.TryGetValue(unit, out var ui))
            return;

        ui.statusBar.SetStatus(type, stacks);
    }
}
