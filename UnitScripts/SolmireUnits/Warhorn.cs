using System.Collections.Generic;
using UnityEngine;

public class Warhorn : UnitInstance
{
    int attackBuff = 10;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        attackBuff = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        attackBuff = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 40,
            _ => 10
        };
    }

    protected override void UseAbility()
    {

        List<UnitInstance> targets = FindAllAllies();
        foreach (UnitInstance target in targets)
        {
            target.TemporaryStatModify(ModifiableStats.Attack, attackBuff);
        }
        base.UseAbility();
    }

    public override string GetActiveDescription()
    {
        return ($"Increase attack of allies by {attackBuff}.");
    }
}
