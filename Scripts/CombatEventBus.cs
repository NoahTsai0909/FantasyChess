public static class CombatEventBus
{
    public enum CombatEventType
    {
        AbilityUsed,
        DamageTaken,
        Healed,
        UnitDied,
    }

    public delegate void CombatEventHandler(CombatEventType type, UnitInstance source, UnitInstance target);

    public static event CombatEventHandler OnCombatEvent;

    public static void Publish(CombatEventType type, UnitInstance source, UnitInstance target)
    {
        OnCombatEvent?.Invoke(type, source, target);
    }
}