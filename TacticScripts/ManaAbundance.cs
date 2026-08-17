using UnityEngine;

public class ManaAbundance : TacticInstance
{
    public int buffValue = 1;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 1,
            Rarity.Uncommon => 2,
            Rarity.Rare => 3,
            Rarity.Epic => 4,
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

    public override void ApplyPassiveEffect()
    {
        base.ApplyPassiveEffect();
        if (allyGrid == null) return;

        auraTargets = FindAllAllies();

        if (auraTargets == null) return;

        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.MaxEnergy, buffValue);
            }
        }
    }

    public override void RemovePassiveEffect()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.MaxEnergy, -buffValue);
            }
        }
        base.RemovePassiveEffect();

    }

    public override string GetDescription()
    {
        return $"All allies have [ENERGY] [c_energy]{buffValue}[/c].";
    }
}

