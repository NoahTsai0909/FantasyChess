using UnityEngine;

public class EssenceofWrath : UnitInstance, IConsumable
{
    private int attackModifier = 2;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Common)
        {
            attackModifier = 2;
        }
        if (CurrentRarity == Rarity.Uncommon)
        {
            attackModifier = 4;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            attackModifier = 8;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            attackModifier = 16;
        }
        else
        {
            attackModifier = 2;
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
