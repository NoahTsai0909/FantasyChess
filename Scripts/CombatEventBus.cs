using System;

public static class CombatEventBus
{
    public enum CombatEventType
    {
        AbilityUsed,
        DamageTaken,
        Healed,
        UnitDied,
        ShieldDamaged,
        ActionResolved
    }

    public delegate void CombatEventHandler(CombatEventType type, UnitInstance source, UnitInstance target);

    public static event CombatEventHandler OnCombatEvent;

    public static void Publish(CombatEventType type, UnitInstance source, UnitInstance target)
    {
        OnCombatEvent?.Invoke(type, source, target);
    }

    public static event Action OnCombatEnd;

    public static void PublishCombatEnd()
    {
        OnCombatEnd?.Invoke();
    }
}