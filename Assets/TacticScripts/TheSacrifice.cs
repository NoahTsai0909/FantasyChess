using System.Collections.Generic;
using UnityEngine;

public class TheSacrifice : TacticInstance
{
    public int buffValue = 2;
    private bool firstTimeTriggered = false;


    public override void EnterCombat()
    {
        base.EnterCombat();
        CombatEventBus.OnCombatEvent += OnCombatEvent;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= OnCombatEvent;
    }

    private void OnCombatEvent(CombatEventBus.CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        if (firstTimeTriggered) return;
        if (type == CombatEventBus.CombatEventType.UnitDied && source.isPlayer == this.isPlayer && enemyGrid != null && allyGrid != null)
        {
            List<UnitInstance> targets = FindAllEnemies();
            List<UnitInstance> allies = FindAllAllies();
            foreach(UnitInstance enemy in targets)
            {
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.ApplyHaste,
                        source = null, // Null because Tactics aren't physical units!
                        target = enemy,
                        amount = buffValue,
                        reason = tacticName
                    });
                }
            }
            foreach (UnitInstance ally in allies)
            {
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.ApplyHaste,
                        source = null, // Null because Tactics aren't physical units!
                        target = ally,
                        amount = buffValue,
                        reason = tacticName
                    });
                }
            }
            firstTimeTriggered = true;

        }
    }



    public override string GetDescription()
    {
        return $"When an ally dies for the first time, [c_haste]haste[/c] ALL units for [HASTE] {buffValue}.";
    }
}
