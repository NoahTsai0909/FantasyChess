using UnityEngine;

public class Runehide : UnitInstance, IConsumable
{
    public int buffValue = 4;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 4,
            Rarity.Uncommon => 8,
            Rarity.Rare => 16,
            Rarity.Epic => 32,
            _ => 4
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
        RunManager.Instance.GetPermanentStatsForUnit(target.id).bonusShield += buffValue;
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
        return ($"Consume this to grant [[c_shield]{buffValue}[/c] [SHIELD] permanently.");
    }
}
