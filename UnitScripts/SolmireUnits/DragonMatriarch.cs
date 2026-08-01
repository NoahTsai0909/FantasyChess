using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DragonMatriarch : UnitInstance
{
    private int attackBuff;
    private int burnBuff;


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
        if (action.type != CombatActionType.ApplyBurn && action.type != CombatActionType.ApplyHaste)
        {
            return;
        }
        if (action.source.isPlayer == this.isPlayer && action.type == CombatActionType.ApplyBurn)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.ApplyHaste,
                    source = this,
                    target = action.source,
                    amount = stats.Haste,
                    reason = "Dragon Matriarch haste"
                }
            );
        }
        if (action.type == CombatActionType.ApplyHaste && action.target.isPlayer == this.isPlayer)
        {
            this.TemporaryStatModify(ModifiableStats.Attack, attackBuff);
            this.TemporaryStatModify(ModifiableStats.Burn, burnBuff);
        }
    }

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        burnBuff = findBurnBuff(CurrentRarity);
        attackBuff = findAttackBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        burnBuff = findBurnBuff(rarity);
        attackBuff = findAttackBuff(rarity);
    }

    private int findBurnBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 4,
            Rarity.Epic => 8,
            _ => 4
        };
    }

    private int findAttackBuff(Rarity rarity)
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
                    reason = "Dragon Matriarch attack",
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
                    reason = "Dragon Matriarch burn",
                    isCrit = abilityCrit
                }
            );
        }
    }



    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack} and [c_burn]burn[/c] it for [BURN] {stats.Burn}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"When an ally [c_burn]burns[/c], [c_haste]haste[/c] it for [HASTE] {stats.Haste}.\nWhen an ally is [c_haste]hasted[/c], this gains [c_attack]{attackBuff}[/c] [ATK] and [c_burn]{burnBuff}[/c] [BURN].");
    }
}
