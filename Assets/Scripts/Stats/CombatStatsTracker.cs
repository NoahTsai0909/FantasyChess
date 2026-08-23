using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatStatsTracker : MonoBehaviour
{
    public static CombatStatsTracker Instance { get; private set; }

    // The master dictionary keeping track of everyone's stats by their GUID
    private Dictionary<Guid, UnitCombatStats> unitStats = new Dictionary<Guid, UnitCombatStats>();

    // An event we can fire so the UI knows exactly when to update the bar charts
    public event Action OnStatsUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        CombatEventBus.OnActionResolved += HandleActionResolved;
    }

    private void OnDisable()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    /// <summary>
    /// Call this at the start of a battle to pre-register everyone on the board.
    /// This ensures we have their Names and IsPlayer status saved before they potentially die.
    /// </summary>
    public void InitializeCombatStats(List<UnitInstance> allStartingUnits)
    {
        unitStats.Clear();
        foreach (var unit in allStartingUnits)
        {
            unitStats[unit.id] = new UnitCombatStats(unit.id, unit.unitName, unit.isPlayer, unit.Definition.unitSprite);
        }
        OnStatsUpdated?.Invoke();
    }

    private void HandleActionResolved(CombatAction action)
    {
        // 1. Ignore UI-only consolidated ticks!
        if (action.isVisualOnly) return;

        // 2. Safely get or create the stats containers
        UnitCombatStats sourceStats = GetOrCreateStats(action.sourceId, action.source);
        UnitCombatStats targetStats = GetOrCreateStats(action.targetId, action.target);

        // 3. Tally the stats based on the action type
        switch (action.type)
        {
            case CombatActionType.Damage:
                if (sourceStats != null) sourceStats.DirectDamageDealt += action.amount;
                if (targetStats != null) targetStats.DamageTaken += action.amount;
                break;

            case CombatActionType.BurnTick:
                if (sourceStats != null) sourceStats.BurnDamageDealt += action.amount;
                if (targetStats != null) targetStats.DamageTaken += action.amount;
                break;

            case CombatActionType.Poison: // Assuming poison ticks use this or a similar action
                if (sourceStats != null) sourceStats.PoisonDamageDealt += action.amount;
                if (targetStats != null) targetStats.DamageTaken += action.amount;
                break;

            case CombatActionType.Heal:
                if (sourceStats != null) sourceStats.HealingDone += action.amount;
                break;

            case CombatActionType.Shield:
                if (sourceStats != null) sourceStats.ShieldingDone += action.amount;
                break;

            case CombatActionType.ApplySlow:
                if (sourceStats != null) sourceStats.SlowsApplied += action.amount;
                break;

            case CombatActionType.ApplyHaste:
                if (sourceStats != null) sourceStats.HastesApplied += action.amount;
                break;

            case CombatActionType.Advance:
                if (sourceStats != null) sourceStats.AdvancesGiven += action.amount;
                break;
        }

        // 4. Tell the UI window that numbers have changed
        OnStatsUpdated?.Invoke();
    }

    private UnitCombatStats GetOrCreateStats(Guid id, UnitInstance unitReference)
    {
        if (id == Guid.Empty) return null;

        if (!unitStats.ContainsKey(id))
        {
            string name = unitReference != null ? unitReference.unitName : "Unknown Entity";
            bool isPlayer = unitReference != null && unitReference.isPlayer;

            // NEW: Grab the sprite from the definition (change 'unitSprite' if yours is named differently)
            Sprite icon = (unitReference != null && unitReference.Definition != null)
                ? unitReference.Definition.unitSprite
                : null;

            unitStats[id] = new UnitCombatStats(id, name, isPlayer, icon);
        }

        return unitStats[id];
    }

    public Dictionary<Guid, UnitCombatStats> GetAllStats()
    {
        return unitStats;
    }
}
