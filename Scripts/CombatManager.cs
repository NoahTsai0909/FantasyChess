using System.Collections.Generic;
using UnityEngine;
using System;

public enum CombatActionType
{
    Damage,
    Heal,
    Shield,
    ApplyBurn,
    BurnTick,
    Poison,
    ApplyHaste,
    ApplySlow,
    Charge,
    Freeze,
    Buff,
    Debuff,
    Advance,
    Kill,
}

public class CombatAction
{
    public CombatActionType type;
    public UnitInstance source;
    public UnitInstance target;
    public Guid sourceId;//Persistent IDs for stat tracking
    public Guid targetId;
    public int amount;
    public bool isCrit;
    public string reason; // optional (ability name, etc)
    public bool isPassive; // optional (for passive abilities)
    public GameObject projectileOverride; // Optional override
    public bool isSilent = false; // Optional flag for no floating combat text UI (for instance, individually attributed burn ticks)
    public bool isVisualOnly = false; // Optional flag for no stat tracking or combat log (for instance, consolidated burn damage)
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [SerializeField] private GridManager playerGrid;
    [SerializeField] private GridManager enemyGrid;

    private List<CombatAction> combatLog = new();

    private float dotTimer = 1f;

    void Awake()
    {
        Instance = this;
    }

    public void ExecuteAction(CombatAction action)
    {
        if (action.source != null) action.sourceId = action.source.id;
        if (action.target != null) action.targetId = action.target.id;
        // Target redirection hook
        action.target = ResolveTargetRedirects(action);

        CombatVFXManager.Instance.PlayActionVFX(
        action,
        () => ResolveAction(action)
        );
    }

    public void ResolveAction(CombatAction action)
    {

        if (action.isCrit)
        {
            switch (action.type)
            {
                case CombatActionType.Damage:
                case CombatActionType.BurnTick:
                case CombatActionType.Heal:
                case CombatActionType.Shield:
                case CombatActionType.ApplyBurn:
                    action.amount *= 2;
                    break;
            }
        }
        switch (action.type)
        {
            case CombatActionType.Damage:
            case CombatActionType.BurnTick:
                action.target.TakeDamage(action.amount);
                break;

            case CombatActionType.Heal:
                action.target.HealDamage(action.amount);
                break;

            case CombatActionType.Shield:
                action.target.ShieldDamage(action.amount);
                break;

            case CombatActionType.ApplyBurn:
                action.target.ApplyBurn(action.amount, action.sourceId);
                break;

            case CombatActionType.ApplySlow:
                action.target.ApplySlow(action.amount);
                break;

            case CombatActionType.ApplyHaste:
                action.target.ApplyHaste(action.amount);
                break;

            case CombatActionType.Advance:
                action.target.Advance(action.amount);
                break;

            case CombatActionType.Kill:
                action.target.Die();
                break;
        }

        combatLog.Add(action);//record the action in the combat log
        CombatEventBus.PublishActionResolved(action); //publish the action resolved event
    }

    void Update()
    {
        dotTimer -= Time.deltaTime;
        if (dotTimer <= 0f)
        {
            TickDots();
            dotTimer = 1f;
        }
    }

    private void TickDots()
    {
        foreach (var unit in GetAllUnitsInCombat())
        {
            if (unit.burnSources.Count > 0)
            {
                int totalBurnDamage = unit.TotalBurnStacks;

                unit.TakeDamage(totalBurnDamage);

                CombatAction visualTracker = new CombatAction
                {
                    type = CombatActionType.BurnTick,
                    target = unit,
                    targetId = unit.id,
                    amount = totalBurnDamage,
                    reason = "Burn tick", // do not log the consolidated damage!,
                    isVisualOnly = true // The stat tracker will completely ignore this!

                };
                CombatEventBus.PublishActionResolved(visualTracker);

                //Stat Tracking: Broadcast split logs silently for the Stats Tracker
                foreach (var kvp in unit.burnSources)
                {
                    CombatAction statTracker = new CombatAction
                    {
                        type = CombatActionType.BurnTick,
                        sourceId = kvp.Key,
                        target = unit,
                        targetId = unit.id,
                        amount = kvp.Value,
                        reason = "Burn",
                        isSilent = true // The UI will completely ignore this!
                    };
                    combatLog.Add(statTracker);
                    CombatEventBus.PublishActionResolved(statTracker);
                }

                //Decay the largest stack
                unit.DecayBurn();

                //Update the UI using the new helper property
                CombatEventBus.PublishStatusChanged(
                    unit,
                    StatusEffectType.Burn,
                    unit.TotalBurnStacks
                );
            }

            if (unit.slowStacks != 0)
            {
                unit.slowStacks--;
                if (unit.slowStacks <= 0)
                    unit.slowStacks = 0;
                CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Slow, unit.slowStacks);
            }

            if (unit.hasteStacks != 0)
            {
                unit.hasteStacks--;
                if (unit.hasteStacks <= 0) unit.hasteStacks = 0;
                CombatEventBus.PublishStatusChanged(unit, StatusEffectType.Haste, unit.hasteStacks);
            }
        }
    }

    private IEnumerable<UnitInstance> GetAllUnitsInCombat()
    {
        if (playerGrid != null)
        {
            foreach (var unit in playerGrid.GetAllUnits())
                yield return unit;
        }

        if (enemyGrid != null)
        {
            foreach (var unit in enemyGrid.GetAllUnits())
                yield return unit;
        }
    }



    private UnitInstance ResolveTargetRedirects(CombatAction action)
    {
        // Future: taunt, intercept, etc
        return action.target;
    }

    public IReadOnlyList<CombatAction> GetCombatLog() => combatLog;
}

