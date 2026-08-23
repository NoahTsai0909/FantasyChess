using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UnitInstance))]
public class StatusEffectController : MonoBehaviour
{
    private UnitInstance unit;

    [Header("Timers")]
    public float hasteTimer { get; private set; }
    public float slowTimer { get; private set; }

    [Header("Burn Tracking")]
    public Dictionary<Guid, int> burnSources { get; private set; } = new Dictionary<Guid, int>();
    public int TotalBurnStacks => burnSources.Values.Sum();
    private Coroutine burnRoutine;

    void Awake()
    {
        unit = GetComponent<UnitInstance>();
    }

    void Update()
    {
        if (unit == null || !unit.inCombat) return;

        // 1. Precise Float Timer for Haste
        if (hasteTimer > 0)
        {
            hasteTimer -= Time.deltaTime;
            // Update UI to show the rounded-up seconds left (e.g. 2s -> 1s)
            CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Haste, Mathf.CeilToInt(hasteTimer));

            if (hasteTimer <= 0)
            {
                hasteTimer = 0;
                CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Haste, 0); // Turn off icon
            }
        }

        // 2. Precise Float Timer for Slow
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Slow, Mathf.CeilToInt(slowTimer));

            if (slowTimer <= 0)
            {
                slowTimer = 0;
                CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Slow, 0); // Turn off icon
            }
        }
    }

    public void AddHaste(float duration) => hasteTimer += duration;
    public void AddSlow(float duration) => slowTimer += duration;

    // --- COMPLEX BURN LOGIC ---

    public void AddBurn(int amount, Guid sourceId)
    {
        if (burnSources.ContainsKey(sourceId))
            burnSources[sourceId] += amount;
        else
            burnSources[sourceId] = amount;

        CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Burn, TotalBurnStacks);

        // If not already burning, start the local routine!
        if (burnRoutine == null)
            burnRoutine = StartCoroutine(BurnTickRoutine());
    }

    private IEnumerator BurnTickRoutine()
    {
        while (TotalBurnStacks > 0)
        {
            // Wait exactly 1 second BEFORE applying damage
            yield return new WaitForSeconds(1f);

            if (unit != null && unit.inCombat)
            {
                int totalBurnDamage = TotalBurnStacks;
                unit.TakeDamage(totalBurnDamage);

                // 1. Visual Only Tracker (For UI popups)
                CombatAction visualTracker = new CombatAction
                {
                    type = CombatActionType.BurnTick,
                    target = unit,
                    targetId = unit.id,
                    amount = totalBurnDamage,
                    reason = "Burn tick",
                    isVisualOnly = true
                };
                CombatEventBus.PublishActionResolved(visualTracker);

                // 2. Silent Logs for Stat Tracker
                foreach (var kvp in burnSources)
                {
                    CombatAction statTracker = new CombatAction
                    {
                        type = CombatActionType.BurnTick,
                        sourceId = kvp.Key,
                        target = unit,
                        targetId = unit.id,
                        amount = kvp.Value,
                        reason = "Burn",
                        isSilent = true
                    };

                    // Safely push to CombatManager's log
                    if (CombatManager.Instance != null)
                        CombatManager.Instance.RecordStatAction(statTracker);

                    CombatEventBus.PublishActionResolved(statTracker);
                }

                DecayBurn();
                CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Burn, TotalBurnStacks);
            }
        }
        burnRoutine = null;
    }

    private void DecayBurn()
    {
        if (burnSources.Count == 0) return;

        Guid maxKey = Guid.Empty;
        int maxVal = -1;
        bool keyFound = false;

        foreach (var kvp in burnSources)
        {
            if (kvp.Value > maxVal)
            {
                maxVal = kvp.Value;
                maxKey = kvp.Key;
                keyFound = true;
            }
        }
        if (keyFound)
        {
            burnSources[maxKey]--;
            if (burnSources[maxKey] <= 0) burnSources.Remove(maxKey);
        }
    }

    public void ClearAllStatusEffects()
    {
        hasteTimer = 0;
        slowTimer = 0;
        burnSources.Clear();
        if (burnRoutine != null) StopCoroutine(burnRoutine);
        burnRoutine = null;
    }
}