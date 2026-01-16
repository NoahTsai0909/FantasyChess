using System.Collections.Generic;
using UnityEngine;

public enum CombatActionType
{
    Damage,
    Heal,
    Shield,
    Burn,
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

    private List<CombatAction> combatLog = new();

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
                action.target.TakeDamage(action.amount);
                break;

            case CombatActionType.Heal:
                action.target.HealDamage(action.amount);
                break;
            case CombatActionType.Shield:
                action.target.ShieldDamage(action.amount);
                break;
        }

        // Record
        combatLog.Add(action);

        // Notify observers
        CombatEventBus.Publish(
            CombatEventBus.CombatEventType.ActionResolved,
            action.source,
            action.target
        );
    }

    private UnitInstance ResolveTargetRedirects(CombatAction action)
    {
        // Future: taunt, intercept, etc
        return action.target;
    }

    public IReadOnlyList<CombatAction> GetCombatLog() => combatLog;
}

