using System.Collections.Generic;
using UnityEngine;

public class Warhorn : UnitInstance
{
    int attackBuff = 10;
    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Uncommon)
        {
            attackBuff = 10;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            attackBuff = 20;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            attackBuff = 40;
        }
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

    public override string GetAbilityDescription()
    {
        return ($"Increase attack of allies by {attackBuff}.");
    }
}
