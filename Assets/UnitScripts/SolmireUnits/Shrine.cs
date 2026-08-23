using System.Collections.Generic;
using UnityEngine;

public class Shrine : UnitInstance
{
    private int healBuff = 10;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        healBuff = findHealBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        healBuff = findHealBuff(rarity);
    }

    private int findHealBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 20,
            Rarity.Rare => 40,
            Rarity.Epic => 80,
            _ => 10
        };
    }

    public override void RemoveAuras()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.Heal, -healBuff);
            }
        }
        base.RemoveAuras(); 
    }

    public override void ApplyAuras()
    {

        if (myGrid == null) return;

        auraTargets = FindAllAllies();

        if (auraTargets == null) return;

        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.Heal, healBuff);
            }
        }
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        healBuff = findHealBuff(CurrentRarity);
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
                type = CombatActionType.Heal,
                source = this,
                target = target,
                amount = stats.Heal,
                reason = "Shrine Heal",
                isCrit = abilityCrit
            }
        );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_heal]Heals[/c] all [c_adjacent]adjacent[/c] allies for [HEAL] {stats.Heal}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"All allies have [c_heal]+{healBuff}[/c] [HEAL].");
    }
}
