using UnityEngine;

public class Cinderblight : UnitInstance, IConsumable
{
    private int burnModifier = 1;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Common)
        {
            burnModifier = 1;
        }
        if (CurrentRarity == Rarity.Uncommon)
        {
            burnModifier = 2;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            burnModifier = 4;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            burnModifier = 8;
        }
        else
        {
            burnModifier = 1;
        }
    }

    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;
        RunManager.Instance.GetPermanentStatsForUnit(target.id).bonusBurn += burnModifier;
        target.RecalculateStats();
        return true;
    }

    public override string GetActiveDescription()
    {
        return ($"Consume this to grant [c_burn]{burnModifier}[/c] [BURN] permanently.");
    }
}
