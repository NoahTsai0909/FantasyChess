using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SludgeVortex : UnitInstance
{
    private int enemyCount = 3;
    private int attackBuff = 15;

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        base.EnterCombat(grid, row, col, isPlayer);

        CombatEventBus.OnActionResolved += HandleActionResolved;

    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    private void HandleActionResolved(CombatAction action)
    {

        if (action.target.isPlayer == this.isPlayer) return;
        if (action.type != CombatActionType.ApplySlow) return;
        this.TemporaryStatModify(ModifiableStats.Attack, attackBuff);
    }

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        attackBuff = findBuff(CurrentRarity);
    }


    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        attackBuff = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 15,
            Rarity.Epic => 30,
            _ => 15
        };
    }

    protected override void UseAbility()
    {
        base.UseAbility();

        List<UnitInstance> targets = FindNearestEnemies(enemyCount);

        // If no enemies are left, stop here
        if (targets.Count == 0) return;

        // Loop through the list and execute the attack on each one
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = stats.Attack,
                    reason = "Sludge Vortex Attack",
                    isCrit = abilityCrit
                }
            );
        }

        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.ApplySlow,
                    source = this,
                    target = target,
                    amount = stats.Slow,
                    reason = "Sludge Vortex Slow"
                }
            );
        }
    }

    public override string GetAbilityDescription()
    {
        return $"Attack up to {enemyCount} nearest enemies for {stats.Attack} damage. Slow each for {stats.Slow} seconds.\n Passive: When an enemy is slowed, this gains {attackBuff} damage.";
    }
}
