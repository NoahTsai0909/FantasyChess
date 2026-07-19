using UnityEngine;
using System.Collections.Generic;

public class AncientTome : UnitInstance
{
    private int energyBuff;
    List<UnitInstance> targets;

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

    public override string GetActiveDescription()
    {
        return ($"Side allies have +{energyBuff} max energy.");
    }
}
