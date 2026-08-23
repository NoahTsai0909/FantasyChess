using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Renewal : TacticInstance
{
    public int buffValue = 10;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 20,
            Rarity.Rare => 40,
            Rarity.Epic => 80,
            _ => 10
        };
    }

    public override void InitializeFromSaveData(RunManager.TacticSaveData data)
    {
        base.InitializeFromSaveData(data);
        buffValue = GetBuffValue(CurrentRarity);
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        buffValue = GetBuffValue(CurrentRarity);
    }

    public override void ExecuteActiveEffect()
    {
        base.ExecuteActiveEffect();

        List<UnitInstance> allies = FindAllAllies();

        foreach (UnitInstance ally in allies)
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ExecuteAction(new CombatAction
                {
                    type = CombatActionType.Heal,
                    source = null, // Null because Tactics aren't physical units!
                    target = ally,
                    amount = buffValue,
                    reason = tacticName
                });
            }
        }
    }


    public override string GetDescription()
    {
        return ($"[c_heal]Heal[/c] all allies for [HEAL]{buffValue}.");
    }
}
