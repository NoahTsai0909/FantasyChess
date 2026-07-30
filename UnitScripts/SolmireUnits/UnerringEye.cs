using System.Collections.Generic;
using UnityEngine;

public class UnerringEye : UnitInstance
{
    private int hasteModifier = 0;
    private int critModifier = 25;
    private List<UnitInstance> allies;
    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        hasteModifier = findBuff(CurrentRarity);
        critModifier = findCritBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        hasteModifier = findBuff(rarity);
        critModifier = findCritBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 0,
            Rarity.Epic => 1,
            _ => 0
        };
    }

    private int findCritBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 25,
            Rarity.Epic => 50,
            _ => 25
        };
    }

    protected override int GetRarityAdjustedHaste()
    {
        return hasteModifier;
    }


    public override void CombatStartEffect()
    {

        allies = FindAdjacentAllies();


        foreach (UnitInstance ally in allies)
        {
            
               ally.TemporaryStatModify(ModifiableStats.CritChance, critModifier);
            
        }
    }

    public override void Die()
    {
        foreach (UnitInstance ally in allies)
        {

            
            ally.TemporaryStatModify(ModifiableStats.CritChance, -critModifier);
            
        }
        base.Die();
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> targets = FindAdjacentAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyHaste,
                source = this,
                target = target,
                amount = stats.Haste,
                reason = "Unerring Eye Haste"
            }

            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_haste]Haste[/c] adjacent allies for [HASTE] {stats.Haste}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"Combat Start: Adjacent allies have [c_crit]+{critModifier}[/c] [CRIT].");
    }
}
