using UnityEngine;

public class SpiritrootElixir : UnitInstance, IConsumable
{
    private int healthModifier = 4;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Common)
        {
            healthModifier = 4;
        }
        if (CurrentRarity == Rarity.Uncommon)
        {
            healthModifier = 6;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            healthModifier = 16;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            healthModifier = 32;
        }
        else
        {
            healthModifier = 4;
        }
    }

    public void OnConsume(UnitInstance target)
    {
        if (target == null) return;
        RunManager.Instance.GetPermanentStatsForUnit(target.id).bonusMaxHP += healthModifier;
        target.RecalculateStats();
    }

    public override string GetAbilityDescription()
    {
        return ($"Overlay on an ally unit to consume this and grant it {healthModifier} health permanently.");
    }
}
