using System.Collections.Generic;
using UnityEngine;

public class Scout : UnitInstance
{
    private int critBuff;
    List<UnitInstance> targets;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        critBuff = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        critBuff = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 7,
            Rarity.Rare => 15,
            Rarity.Epic => 30,
            _ => 7
        };
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        targets = FindSideAllies();

        foreach (UnitInstance target in targets)
        {

            target.TemporaryStatModify(ModifiableStats.CritChance, critBuff);
        }

    }

    public override string GetActiveDescription()
    {
        return ($"[c_side]Side[/c] allies gain [c_crit]{critBuff}[/c] [CRIT].");
    }
}
