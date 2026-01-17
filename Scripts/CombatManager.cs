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
    Haste,
    Slow,
    Charge,
    Freeze,
    Buff,
    Debuff
}

public class CombatAction
{
    public CombatActionType type;
    public UnitInstance source;
    public UnitInstance target;
    public int amount;
    public string reason; // optional (ability name, etc)
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

        // Execute
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
            if (unit.burnStacks == 0)
                continue;

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

