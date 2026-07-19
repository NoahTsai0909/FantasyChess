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

    public void OnConsume(UnitInstance target)
    {
        if (target == null) return;
        RunManager.Instance.GetPermanentStatsForUnit(target.id).bonusBurn += burnModifier;
        target.RecalculateStats();
    }

    public override string GetActiveDescription()
    {
        return ($"Overlay on an ally unit to consume this and grant it {burnModifier} burn permanently.");
    }
}
