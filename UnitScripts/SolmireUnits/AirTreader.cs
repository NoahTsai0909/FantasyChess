using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AirTreader : UnitInstance
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
            Rarity.Rare => 0,
            Rarity.Epic => 1,
            _ => 0
        };
    }

    protected override void UseAbility()
    {
        if (currentEnergy <= 0) return;
        List<UnitInstance> targets = FindAllAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyHaste,
                source = this,
                target = target,
                amount = stats.Haste,
                reason = "AirTreader Haste"
            }

            );
        }
        base.UseAbility();
    }

    protected override int GetRarityAdjustedHaste()
    {
        return hasteModifier;
    }

    public override string GetAbilityDescription()
    {
        return ($"Hastes all allies for {stats.Haste} seconds.");
    }
}
