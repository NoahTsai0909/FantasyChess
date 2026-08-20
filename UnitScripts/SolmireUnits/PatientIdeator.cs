using UnityEngine;

public class PatientIdeator : UnitInstance
{
    private int maxHealthBuff = 5;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        maxHealthBuff = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        maxHealthBuff = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 5,
            Rarity.Rare => 10,
            Rarity.Epic => 20,
            _ => 20
        };
    }
    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        maxHealthBuff = findBuff(CurrentRarity);
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
        if ((action.type == CombatActionType.Heal) && (action.target.isPlayer == this.isPlayer))
        {
            action.target.TemporaryStatModify(ModifiableStats.MaxHP, maxHealthBuff);
        }
    }
    public override string GetPassiveDescription()
    {
        return ($"When an ally is healed, it also gains [c_maxhealth]+{maxHealthBuff}[/c] [MAXHEALTH]");
    }
}
