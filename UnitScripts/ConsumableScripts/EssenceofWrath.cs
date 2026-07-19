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

    public void OnConsume(UnitInstance target)
    {
        if (target == null) return;
        RunManager.Instance.GetPermanentStatsForUnit(target.id).bonusAttack += attackModifier;
        target.RecalculateStats();
    }

    public override string GetActiveDescription()
    {
        return ($"Overlay on an ally unit to consume this and grant it {attackModifier} attack permanently.");
    }
}
