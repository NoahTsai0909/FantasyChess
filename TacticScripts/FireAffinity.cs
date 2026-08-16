using System.Collections.Generic;
using UnityEngine;

public class FireAffinity : TacticInstance
{
    public int burnBuff = 1;

    private int GetBurnBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 1,
            Rarity.Uncommon => 2,
            Rarity.Rare => 4,
            Rarity.Epic => 8,
            _ => 1
        };
    }

    public override void InitializeFromSaveData(RunManager.TacticSaveData data)
    {
        base.InitializeFromSaveData(data);
        burnBuff = GetBurnBuff(CurrentRarity);
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        burnBuff = GetBurnBuff(CurrentRarity);
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
                target.TemporaryStatModify(ModifiableStats.Burn, burnBuff);
            }
        }
    }

    public override void RemovePassiveEffect()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.Burn, -burnBuff);
            }
        }
        base.RemovePassiveEffect();

    }

    public override string GetDescription()
    {
        return $"All allies have + [BURN] [c_burn]{burnBuff}[/c]."; 
    }
}
