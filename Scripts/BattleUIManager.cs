using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private GameObject cooldownBarPrefab;
    [SerializeField] private Canvas battleCanvas; // Reference to your battle UI Canvas

    private Dictionary<UnitInstance, (GameObject healthBar, Image healthFill, GameObject cooldownBar, Image cooldownFill)>
    unitUIElements = new Dictionary<UnitInstance, (GameObject, Image, GameObject, Image)>();


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

        // Use world position directly (no screen conversion)
        Vector3 uiPosition = worldPosition;

        // Create health bar at unit's world position with offset
        GameObject healthBar = Instantiate(healthBarPrefab, battleCanvas.transform);
        healthBar.transform.position = uiPosition + GetHealthBarOffset(unit.isPlayer);
        healthBar.transform.localScale = Vector3.one * 0.2f;

        // Create cooldown bar
        GameObject cooldownBar = Instantiate(cooldownBarPrefab, battleCanvas.transform);
        cooldownBar.transform.position = uiPosition + GetCooldownBarOffset(unit.isPlayer);
        cooldownBar.transform.localScale = Vector3.one * 0.3f; // Scale it down

        Image healthFill = FindHealthBarImage(healthBar);
        Image cooldownFill = FindCooldownBarImage(cooldownBar);

        // Store references
        unitUIElements[unit] = (healthBar, healthFill, cooldownBar, cooldownFill);

        // Initialize
        if (healthFill != null)
        {
            healthFill.fillAmount = 1f;
            healthFill.color = Color.green;
            Debug.Log("Color of health is green");
        }
        if (cooldownFill != null) cooldownFill.fillAmount = 0f;
    }

    public void UpdateUnitUI(UnitInstance unit, Vector3 worldPosition)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements))
        {
            Vector3 uiPosition = worldPosition;
            uiElements.healthBar.transform.position = uiPosition + GetHealthBarOffset(unit.isPlayer);
            uiElements.cooldownBar.transform.position = uiPosition + GetCooldownBarOffset(unit.isPlayer);
        }
    }

    public void UpdateHealthBar(UnitInstance unit, float fillAmount)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements) && uiElements.healthFill != null)
        {
            uiElements.healthFill.fillAmount = fillAmount;

            // Update color based on health
            if (fillAmount > 0.6f)
                uiElements.healthFill.color = Color.green;
            else if (fillAmount > 0.3f)
                uiElements.healthFill.color = Color.yellow;
            else
                uiElements.healthFill.color = Color.red;
        }
    }

    public void UpdateCooldownBar(UnitInstance unit, float fillAmount)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements) && uiElements.cooldownFill != null)
        {
            uiElements.cooldownFill.fillAmount = fillAmount;
        }
    }

    public void RemoveUnitUI(UnitInstance unit)
    {
        if (unitUIElements.TryGetValue(unit, out var uiElements))
        {
            if (uiElements.healthBar != null)
                Destroy(uiElements.healthBar);
            if (uiElements.cooldownBar != null)
                Destroy(uiElements.cooldownBar);

            unitUIElements.Remove(unit);
        }
    }

    private Vector3 GetHealthBarOffset(bool isPlayer)
    {
        // Adjust Y offset based on sprite height - try larger values
        return new Vector3(0, -1f, 0); // Changed from -0.5f to -1f
    }

    private Vector3 GetCooldownBarOffset(bool isPlayer)
    {
        return new Vector3(-0.8f, -1f, 0); // Changed to be more visible
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
}
