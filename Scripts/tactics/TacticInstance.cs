using System;
using System.Collections.Generic;
using UnityEngine;

public class TacticInstance : MonoBehaviour
{
    [SerializeField] private TacticDefinition definition;
    public TacticDefinition Definition => definition;

    public Rarity CurrentRarity { get; private set; }

    public string tacticName;
    public bool isPassive;

    [Header("Combat State")]
    public bool inCombat = false;
    protected float cooldownTimer;

    [Header("Visuals")]
    public UnityEngine.UI.Image fillIcon;
    public UnityEngine.UI.Image backgroundIcon;

    [Header("Targeting")]
    public bool isPlayer;
    protected GridManager allyGrid;
    protected GridManager enemyGrid;
    protected TargetingSystem targetingSystem;

    public bool isSpent { get; private set; }
    public float GetCooldownTimer() => cooldownTimer;

    public Guid id;
    public TacticBarManager myBar;
    public RunManager.TacticPlacement myPlacement;

    // Optional: If you want visual pulsing/outlines similar to units
    // public TacticVisualController Visuals { get; private set; }

    protected virtual void Awake()
    {
        id = Guid.NewGuid();
        // Visuals = GetComponent<TacticVisualController>();
    }

    private void Start()
    {
        if (definition != null)
        {
            tacticName = definition.tacticName;
            isPassive = definition.isPassive;
            // Visuals?.InitializeVisuals(definition);
        }
    }

    public virtual void InitializeFromSaveData(RunManager.TacticSaveData data)
    {
        if (data == null) return;

        definition = data.definition;
        CurrentRarity = data.rarity;
        isPassive = definition.isPassive;
        id = data.id;

        // Visuals?.UpdateRarityOutline(CurrentRarity);
    }

    public void EnterCombat()
    {
        inCombat = true;
        isSpent = false;
        cooldownTimer = definition.cooldown;

        // Reset visuals for the start of the fight
        if (fillIcon != null)
        {
            fillIcon.color = Color.white;
            fillIcon.fillAmount = 0f; // Starts empty and fills up!
        }

        if (isPassive) ApplyPassiveEffect();
    }

    /* =========================
     * TIER / MERGE LOGIC
     * ========================= */

    public bool CanUpgradeTier()
    {
        return CurrentRarity != Rarity.Epic;
    }

    public void UpgradeTier()
    {
        if (!CanUpgradeTier()) return;

        // Remove passive effects before upgrading, just in case they scale!
        if (isPassive && inCombat) RemovePassiveEffect();

        CurrentRarity = RarityScaling.GetNextRarity(CurrentRarity);

        if (myPlacement != null && myPlacement.tacticData != null)
        {
            myPlacement.tacticData.rarity = CurrentRarity;
        }

        OnTierUpgraded();

        if (isPassive && inCombat) ApplyPassiveEffect();

        // Visuals?.UpdateRarityOutline(CurrentRarity);
    }

    protected virtual void OnTierUpgraded()
    {
        // Override in specific tactics to recalculate variables if needed
    }

    /* =========================
     * COMBAT ACTIONS
     * ========================= */

    /// <summary>
    /// Called manually by the timeline manager when this tactic reaches the front of the queue.
    /// </summary>
    /// 
    public bool TickCooldown(float dt)
    {
        cooldownTimer -= dt;

        // Animate the bottom-up fill
        if (fillIcon != null && definition.cooldown > 0)
        {
            // Fills from 0 to 1 as the timer counts down
            fillIcon.fillAmount = 1f - (cooldownTimer / definition.cooldown);
        }

        return cooldownTimer <= 0;
    }
    public void ResetCooldown() { cooldownTimer = Definition.cooldown; }
    public virtual void ExecuteActiveEffect()
    {
        // Example: FindFirstObjectByType<GridManager>().GetAllUnits().ForEach(u => u.TemporaryStatModify(ModifiableStats.Attack, 5));
    }

    public virtual void ApplyPassiveEffect()
    {
        // Override for always-on auras
    }

    public virtual void RemovePassiveEffect()
    {
        // Override to clean up auras
    }

    /// <summary>
    /// Helps dynamically scale numbers based on the tactic's current rarity.
    /// </summary>
    protected float GetRarityMultiplier()
    {
        int delta = CurrentRarity - Definition.startingRarity;
        return RarityScaling.GetMultiplier(delta);
    }

    public void MarkAsSpent()
    {
        isSpent = true;

        // Gray out the icon to show it is out of commission for this combat
        if (fillIcon != null)
        {
            fillIcon.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark gray
        }
    }

    public virtual void SetupTargeting(bool isPlayerSide)
    {
        isPlayer = isPlayerSide;
        gameManager gm = FindFirstObjectByType<gameManager>();

        if (gm != null)
        {
            allyGrid = isPlayer ? gm.playerGrid : gm.enemyGrid;
            enemyGrid = isPlayer ? gm.enemyGrid : gm.playerGrid;
            targetingSystem = new TargetingSystem(allyGrid, enemyGrid, isPlayer);
        }
    }

    // --- Targeting Helpers exactly like UnitInstance ---

    protected List<UnitInstance> FindAllAllies()
    {
        if (allyGrid != null) return allyGrid.GetAllUnits();
        return new List<UnitInstance>();
    }

    protected List<UnitInstance> FindAllEnemies()
    {
        if (targetingSystem != null) return targetingSystem.GetEnemies();
        return new List<UnitInstance>();
    }

    protected UnitInstance FindNearestEnemy()
    {
        if (targetingSystem == null) return null;
        var criteria = new TargetingSystem.TargetCriteria(
            TargetingSystem.TargetTeam.Enemy,
            TargetingSystem.SortMethod.Nearest
        );
        return targetingSystem.FindUnit(criteria, transform.position);
    }

    protected UnitInstance FindLowestHealthAlly()
    {
        if (targetingSystem == null) return null;
        var criteria = new TargetingSystem.TargetCriteria(
            TargetingSystem.TargetTeam.Ally,
            TargetingSystem.SortMethod.LowestHealth
        );
        return targetingSystem.FindUnit(criteria, transform.position);
    }
}