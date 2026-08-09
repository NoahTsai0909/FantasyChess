using UnityEngine;

public class SpiritrootElixir : UnitInstance, IConsumable
{
    private int healthModifier = 4;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Common)
        {
            healthModifier = 10;
        }
        if (CurrentRarity == Rarity.Uncommon)
        {
            healthModifier = 20;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            healthModifier = 40;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            healthModifier = 80;
        }
        else
        {
            healthModifier = 10;
        }
    }

    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;
        RunManager.Instance.GetPermanentStatsForUnit(target.id).bonusMaxHP += healthModifier;
        target.RecalculateStats();
        return true;
    }

    public override string GetActiveDescription()
    {
        return ($"Consume this to grant [c_maxhealth]{healthModifier}[/c] [MAXHEALTH] permanently.");
    }
}
