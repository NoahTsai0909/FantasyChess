using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ScorchingCaster : UnitInstance
{
    private int enemyCount = 2;
    private int burnBuff = 2;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        burnBuff = findBurnBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        burnBuff = findBurnBuff(rarity);
    }

    private int findBurnBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 2,
            Rarity.Rare => 4,
            Rarity.Epic => 8,
            _ => 2
        };
    }

    protected override void UseAbility()
    {
        base.UseAbility();

        List<UnitInstance> targets = FindNearestEnemies(enemyCount);
        if (targets.Count == 0) return;

        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Scorching Caster Attack",
                isCrit = abilityCrit
            }
            );

            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.ApplyBurn,
                source = this,
                target = target,
                amount = stats.Burn,
                reason = "Scorching Caster Burn",
                isCrit = abilityCrit
            }
        );

        }
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
        if (action.source == null) return;
        if (action.source.isPlayer != this.isPlayer || action.source.isEnergy == false) return;
        this.TemporaryStatModify(ModifiableStats.Burn, burnBuff);
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] up to {enemyCount} nearest enemes for [ATK] {stats.Attack}. [c_burn]Burn[/c] them for [BURN] {stats.Burn}." );
    }

    public override string GetPassiveDescription()
    {
        return ($"When an ally uses [ENERGY], get [c_burn]+{burnBuff}[/c] [BURN].");
    }
}
