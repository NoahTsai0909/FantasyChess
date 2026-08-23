using UnityEngine;

public class EssenceofWrath : UnitInstance, IConsumable
{
    private int attackModifier = 4;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Common)
        {
            attackModifier = 4;
        }
        if (CurrentRarity == Rarity.Uncommon)
        {
            attackModifier = 8;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            attackModifier = 16;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            attackModifier = 32;
        }
        else
        {
            attackModifier = 4;
        }
    }

    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;
        RunManager.Instance.GetPermanentStatsForUnit(target.id).bonusAttack += attackModifier;
        target.RecalculateStats();
        return true;
    }

    public override string GetActiveDescription()
    {
        return ($"Consume this to grant [c_attack]{attackModifier}[/c] [ATK] permanently.");
    }
}
