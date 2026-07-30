using System.Collections.Generic;
using UnityEngine;

public class Lightmare : UnitInstance
{

    private int hasteModifier = 0;
    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        hasteModifier = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        hasteModifier = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 0,
            Rarity.Rare => 1,
            Rarity.Epic => 2,
            _ => 0
        };
    }

    protected override int GetRarityAdjustedHaste()
    {
        return hasteModifier;
    }

    protected override void UseAbility()
    {

        List<UnitInstance> targets = FindSideAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyHaste,
                source = this,
                target = target,
                amount = stats.Haste,
                reason = "Lightmare Haste"
            }

            );
        }
        base.UseAbility();
    }

    public override string GetActiveDescription()
    {
        return ($"[c_haste]Haste[/c] side allies for [HASTE] {stats.Haste}.");
    }
}
