using System.Collections.Generic;
using UnityEngine;

public class BrotherInArms : TacticInstance
{
    public int buffValue = 10;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 5,
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 40,
            _ => 5
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
            int adjacentAlliesCount = ally.FindAdjacentAllies().Count;
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ExecuteAction(new CombatAction
                {
                    type = CombatActionType.Shield,
                    source = null, 
                    target = ally,
                    amount = buffValue * adjacentAlliesCount,
                    reason = tacticName
                });
            }
        }
    }


    public override string GetDescription()
    {
        return ($"[c_shield]Shield[/c] all allies for [SHIELD]{buffValue} for each adjacent ally.");
    }
}
