using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FanTheFlame : TacticInstance
{
    public int hasteStacks = 1;

    public override void ExecuteActiveEffect()
    {
        base.ExecuteActiveEffect();

        List<UnitInstance> allies = FindAllAllies();

        List<UnitInstance> burnAllies = allies.Where(unit =>
            unit != null &&
            unit.Definition != null &&
            unit.Definition.tagFlags.HasFlag(UnitTagFlags.Burn)).ToList();

        if (burnAllies.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, burnAllies.Count);
            UnitInstance target = burnAllies[randomIndex];

            // Route through CombatManager exactly like a Unit does!
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ExecuteAction(new CombatAction
                {
                    type = CombatActionType.ApplyHaste,
                    source = null, // Null because Tactics aren't physical units!
                    target = target,
                    amount = hasteStacks,
                    reason = tacticName
                });
            }

            Debug.Log($"<color=orange>{tacticName}</color> fired! Queued {hasteStacks} Haste for {target.unitName}.");
        }
        else
        {
            Debug.Log($"<color=orange>{tacticName}</color> fired, but no Burn units were found on the board!");
        }
    }
}
