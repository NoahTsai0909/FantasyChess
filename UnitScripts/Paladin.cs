using System.Collections.Generic;
using UnityEngine;
using static CombatEventBus;

public class Paladin : UnitInstance 
{
    private int attackModifier = 4;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Uncommon)
        {
            attackModifier = 4;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            attackModifier = 8;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            attackModifier = 16;
        }
        else
        {
            attackModifier = 4;
        }
    }
    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        base.EnterCombat(grid, row, col, isPlayer);
        CombatEventBus.OnActionResolved += HandleCombatAction;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleCombatAction;
    }

    protected override void HandleCombatAction(CombatAction action)
    {
        if ((action.type == CombatActionType.Heal) && (action.target.isPlayer == this.isPlayer))
        {
            TemporaryStatModify(ModifiableStats.Attack, attackModifier);
        }
    }

    protected override void UseAbility()
    {
        UnitInstance target = FindNearestEnemy();

        if (target != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = stats.Attack,
                    reason = "Paladin Attack"
                }
            );
        }
        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Passive: When an ally is healed, this unit gains {attackModifier} attack for this fight. Attacks the nearest enemy for {stats.Attack} damage.");
    }
}
