using UnityEngine;
using static CombatEventBus;

public class Ballista : UnitInstance
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Ballista deployed");
        if (isPassive)
            CombatEventBus.OnCombatEvent += HandleCombatEvent;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= HandleCombatEvent;
    }

    protected override void HandleCombatEvent(CombatEventType type, UnitInstance source, UnitInstance target)
    {
        // Only care about ability use events
        if (type != CombatEventType.AbilityUsed) return;

        // Must be ally
        if (source.isPlayer != this.isPlayer) return;

        // Must be exactly one tile in front
        int expectedCol = isPlayer ? col + 1 : col - 1;
        if (source.row != row || source.col != expectedCol) return;

        // Now fire!
        UnitInstance enemy = FindNearestEnemy();
        if (enemy != null)
        {
            Debug.Log($"{unitName} fires due to ally {source.unitName}!");
            enemy.TakeDamage(attackValue);
        }
    }
}
