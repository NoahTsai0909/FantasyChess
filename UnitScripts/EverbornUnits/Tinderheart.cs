using System.Collections.Generic;
using UnityEngine;

public class Tinderheart : UnitInstance
{
    private int burnBuff = 2;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        burnBuff = findBuff(CurrentRarity);
    }
    

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        burnBuff = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 2,
            Rarity.Rare => 4,
            Rarity.Epic => 8,
            _ => 2
        };
    }
    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> targets = FindAllAllies();
        foreach (UnitInstance target in targets)
        {
            target.TemporaryStatModify(ModifiableStats.Burn, burnBuff);
        }
    }

    public override string GetAbilityDescription()
    {
        return ($"Allies gain {burnBuff} burn.");
    }
}
