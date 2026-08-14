using System;
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
        cooldownTimer = definition.cooldown;

        if (isPassive)
        {
            ApplyPassiveEffect();
        }
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
    public bool TickCooldown(float dt) { cooldownTimer -= dt; return cooldownTimer <= 0; }
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
}