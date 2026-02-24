using UnityEngine;
using System.Collections.Generic;

public class AncientTome : UnitInstance
{
    private int energyBuff;
    List<UnitInstance> targets;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Uncommon)
        {
            energyBuff = 1;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            energyBuff = 2;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            energyBuff = 3;
        }
        else
        {
            energyBuff = 1;
        }
    }

    public override void CombatStartEffect()
    {
        targets = FindSideAllies();

        foreach (UnitInstance target in targets)
        {

            target.TemporaryStatModify(ModifiableStats.MaxEnergy, energyBuff);
        }
    }

    public override void Die()
    {
        foreach (UnitInstance target in targets)
        {
            target.TemporaryStatModify(ModifiableStats.MaxEnergy, -energyBuff);
        }
        base.Die();
    }

    public override string GetAbilityDescription()
    {
        return ($"Side allies have +{energyBuff} max energy.");
    }
}
