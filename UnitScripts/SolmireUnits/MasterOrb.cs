using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MasterOrb : UnitInstance
{
    List<UnitInstance> allies = new List<UnitInstance>();
    public override void CombatStartEffect()
    {

        allies = FindAllAllies();


        foreach (UnitInstance ally in allies)
        {
            if (ally.isEnergy)
            {
                ally.TemporaryStatModify(ModifiableStats.MaxEnergy, 4);
            }
        }
    }

    public override void Die()
    {
        foreach (UnitInstance ally in allies)
        {
            if (ally.isEnergy)
            {
                ally.TemporaryStatModify(ModifiableStats.MaxEnergy, -4);
            }
        }
        base.Die();
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
        return ("Combat Start: Allies have [c_energy]+4[/c] [ENERGY]. Whenever a [ENERGY] is used, [c_attack]attack[/c] the farthest enemy by [ATK] {stats.Attack}.");
    }
}
