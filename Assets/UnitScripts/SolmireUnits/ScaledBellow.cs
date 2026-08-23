using UnityEngine;

public class ScaledBellow : UnitInstance
{
    private int attackModifier = 10;
    private int burnModifier = 2;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        attackModifier = findBuff(CurrentRarity);
        burnModifier = findBurnBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        attackModifier = findBuff(rarity);
        burnModifier = findBurnBuff(CurrentRarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 30,
            _ => 30
        };
    }
    private int findBurnBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 2,
            Rarity.Rare => 4,
            Rarity.Epic => 6,
            _ => 2
        };
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        attackModifier = findBuff(CurrentRarity);
    }

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);
        CombatEventBus.OnActionResolved += HandleCombatAction;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleCombatAction;
    }

    protected override void HandleCombatAction(CombatAction action)
    {
        if (action.source == null) return;
        if (action.source.isPlayer != this.isPlayer) return;
        if (action.type == CombatActionType.ApplyBurn)
        {
            action.source.TemporaryStatModify(ModifiableStats.Burn, burnModifier);
        }
        if (action.type == CombatActionType.Damage){
            action.source.TemporaryStatModify(ModifiableStats.Attack, attackModifier);
        }
    }

    public override string GetPassiveDescription()
    {
        return ($"When an ally [c_attack]attacks[/c], give it [ATK] {attackModifier}. \n When an ally [c_burn]burns[/c], give it [BURN] {burnModifier}.");
    }
}

