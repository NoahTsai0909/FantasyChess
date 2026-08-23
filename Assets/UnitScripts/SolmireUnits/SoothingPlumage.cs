using System.Collections.Generic;
using UnityEngine;

public class SoothingPlumage : UnitInstance
{
    private int buffValue = 5;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        buffValue = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        buffValue = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 5,
            Rarity.Rare => 10,
            Rarity.Epic => 20,
            _ => 5
        };
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        buffValue = findBuff(CurrentRarity);
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
        if (action.type != CombatActionType.Heal || action.target.isPlayer != this.isPlayer) return;
        TemporaryStatModify(ModifiableStats.Heal, buffValue);
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        List<UnitInstance> targets = FindAllAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Heal,
                source = this,
                target = target,
                amount = stats.Heal,
                reason = "Soothing Plumage Heal",
                isCrit = abilityCrit
            }

            );
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_heal]Heal[/c] all allies for [HEAL] {stats.Heal}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"When an ally is [c_heal]healed[/c], get [HEAL] {buffValue}.");
    }
}

