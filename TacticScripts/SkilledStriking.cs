using UnityEngine;

public class SkilledStriking : TacticInstance
{
    public int buffValue = 5;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 5,
            Rarity.Uncommon => 10,
            Rarity.Rare => 15,
            Rarity.Epic => 20,
            _ => 5
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
                target.TemporaryStatModify(ModifiableStats.CritChance, buffValue);
            }
        }
    }

    public override void RemovePassiveEffect()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.CritChance, -buffValue);
            }
        }
        base.RemovePassiveEffect();

    }

    public override string GetDescription()
    {
        return $"All allies have [CRIT] [c_crit]{buffValue}[/c].";
    }
}

