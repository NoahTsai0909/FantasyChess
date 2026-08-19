using System.Collections.Generic;
using UnityEngine;

public class MaximumHeal : TacticInstance
{

    public override void ExecuteActiveEffect()
    {
        base.ExecuteActiveEffect();

        List<UnitInstance> allies = FindAllAllies();

        foreach (UnitInstance ally in allies)
        {
            int missingHealth = ally.GetMaxHP() - ally.GetCurrentHP();
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ExecuteAction(new CombatAction
                {
                    type = CombatActionType.Shield,
                    source = null, // Null because Tactics aren't physical units!
                    target = ally,
                    amount = missingHealth,
                    reason = tacticName
                });
            }
        }
    }


    public override string GetDescription()
    {
        return ($"[c_heal]Heal[/c] all allies to full health.");
    }
}