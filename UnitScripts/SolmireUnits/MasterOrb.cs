using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MasterOrb : UnitInstance
{
    private int energyBuff = 3;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        energyBuff = findEnergyBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        energyBuff = findEnergyBuff(rarity);
    }

    private int findEnergyBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 3,
            Rarity.Epic => 4,
            _ => 3
        };
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

        auraTargets = FindAdjacentAllies();

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
        energyBuff = findEnergyBuff(CurrentRarity);
    }


    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);

        CombatEventBus.OnActionResolved += HandleActionResolved;

    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    private void HandleActionResolved(CombatAction action)
    {

        if (action.source.isPlayer != this.isPlayer || action.source.isEnergy == false) return;

        UnitInstance target = FindFarthestEnemy();
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Master Orb Passive",
                isPassive = true
            }
        );
    }

    public override string GetPassiveDescription()
    {
        return ($"Allies have [c_energy]+{energyBuff}[/c] [ENERGY]. When an ally uses [ENERGY], [c_attack]attack[/c] the farthest enemy by [ATK] {stats.Attack}.");
    }
}
