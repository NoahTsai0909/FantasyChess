using UnityEngine;
using System.Collections.Generic;

public class AncientTome : UnitInstance
{
    private int energyBuff;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        energyBuff = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        energyBuff = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 1,
            Rarity.Rare => 2,
            Rarity.Epic => 4,
            _ => 1
        };
    }

    public override string GetPassiveDescription()
    {
        return ($"Side allies have [c_energy]+{energyBuff}[/c] max [ENERGY].");
    }

    public override void RemoveAuras()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.MaxEnergy, -energyBuff);
            }
        }
        base.RemoveAuras(); // Clears the list
    }

    public override void ApplyAuras()
    {

        if (myGrid == null) return;

        auraTargets = FindSideAllies();

        if (auraTargets == null) return;

        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.MaxEnergy, energyBuff);
            }
        }
    }
}
