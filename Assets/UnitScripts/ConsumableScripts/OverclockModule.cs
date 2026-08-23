using UnityEngine;

public class OverclockModule : UnitInstance, IConsumable
{
    public int buffValue = 3;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 3,
            Rarity.Rare => 6,
            Rarity.Epic => 9,
            _ => 3
        };
    }

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        buffValue = GetBuffValue(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        buffValue = GetBuffValue(rarity);
    }

    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;
        RunManager.Instance.GetPermanentStatsForUnit(target.id).cooldownReduction += buffValue;
        target.RecalculateStats();
        return true;
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        buffValue = GetBuffValue(CurrentRarity);
    }

    public override string GetActiveDescription()
    {
        return ($"Consume this to reduce target cooldown by {buffValue}% permanently.");
    }
}
