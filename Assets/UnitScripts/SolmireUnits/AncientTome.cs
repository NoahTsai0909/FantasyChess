using System.Collections.Generic;
using UnityEngine;
using static CombatEventBus;

public class AncientTome : UnitInstance
{
    private int energyBuff;

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

    public override string GetPassiveDescription()
    {
        return ($"[c_side]Side[/c] allies have [c_energy]+{energyBuff}[/c] max [ENERGY].");
    }

    public override string GetMutationTriggerText()
    {
        return ($"<br>When an [ENERGY] ally uses an ability, ");
    }
    public override void RemoveAuras()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.MaxEnergy, -energyBuff);
            }
        }
        base.RemoveAuras(); // Clears the list
    }

    public override void ApplyAuras()
    {

        if (myGrid == null) return;

        auraTargets = FindSideAllies();

        if (auraTargets == null) return;

        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.MaxEnergy, energyBuff);
            }
        }
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        energyBuff = findBuff(CurrentRarity);
    }

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);

        CombatEventBus.OnCombatEvent += HandleCombatEvent;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= HandleCombatEvent;
    }

    protected override void HandleCombatEvent(CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        if (type != CombatEventType.AbilityUsed) return;
        if (source.isPlayer != this.isPlayer) return;
        if (source.isEnergy == false) return;

        if (this != null && inCombat && currentSuffix != null)
        {
            currentSuffix.ExecuteEffect(this);
        }
    }

}
