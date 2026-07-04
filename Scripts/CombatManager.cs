using System.Collections.Generic;
using UnityEngine;

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
    public int amount;
    public string reason; // optional (ability name, etc)
    public Sprite projectileOverride; // Optional override
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
        // Target redirection hook
        action.target = ResolveTargetRedirects(action);

        CombatVFXManager.Instance.PlayActionVFX(
        action,
        () => ResolveAction(action)
        );
    }

    public void ResolveAction(CombatAction action)
    {
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
                action.target.ApplyBurn(action.amount);
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

        // Record
        combatLog.Add(action);

        // Notify observers
        CombatEventBus.PublishActionResolved(action);
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
            if (unit.burnStacks != 0)
            {
                CombatAction action = new CombatAction
                {
                    type = CombatActionType.BurnTick,
                    source = unit, // or burn applier
                    target = unit,
                    amount = unit.burnStacks,
                    reason = "Burn"
                };

                ExecuteAction(action);

                unit.burnStacks--;

                if (unit.burnStacks <= 0)
                    unit.burnStacks = 0;

                CombatEventBus.PublishStatusChanged(
                    unit,
                    StatusEffectType.Burn,
                    unit.burnStacks
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

