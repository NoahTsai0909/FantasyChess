using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class MagicalReserves : TacticInstance
{
    public int buffValue = 1;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 1,
            Rarity.Epic => 2,
            _ => 1
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

        List<UnitInstance> allies = FindAllAllies().Where(unit => unit != null && unit.Definition != null && unit.Definition.isEnergy).ToList();

        foreach (UnitInstance ally in allies)
        {
            ally.currentEnergy = Mathf.Min(ally.currentEnergy + 1, ally.Stats.maxEnergy);
        }
    }


    public override string GetDescription()
    {
        return ($"Recharge all allies [ENERGY] {buffValue}.");
    }
}
