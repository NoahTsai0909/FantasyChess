using Unity.VisualScripting;
using UnityEngine;

public class IroncladSentinel : UnitInstance
{
    private int critBuff = 75;
    public bool isShielded = false;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        critBuff = findCritBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        critBuff = findCritBuff(rarity);
    }

    private int findCritBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 75,
            Rarity.Epic => 100,
            _ => 75
        };
    }
    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        critBuff = findCritBuff(CurrentRarity);
    }

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);
        isShielded = this.GetCurrentShield() > 0;
        CombatEventBus.OnActionResolved += HandleActionResolved;

    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    private void HandleActionResolved(CombatAction action)
    {
        if ((action.target == this && action.type == CombatActionType.Damage) || (action.target == this && action.type == CombatActionType.Shield))
        {
            if (isShielded && this.GetCurrentShield() <= 0)
            {
                isShielded = false;
                this.TemporaryStatModify(ModifiableStats.CritChance, -critBuff);
            }
            else if (!isShielded && this.GetCurrentShield() > 0)
            {
                isShielded = true;
                this.TemporaryStatModify(ModifiableStats.CritChance, critBuff);
            }
        }
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        if (target == null) return;
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Ironclad Sentinel Attack",
                isCrit = abilityCrit
            }
            );

            int shieldAmount = stats.Attack;
            if (abilityCrit)
            {
                shieldAmount *= 2;
            }

            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = this,
                amount = shieldAmount,
                reason = "Ironclad Sentinel Shield",
                isCrit = abilityCrit
            }
            );
        }


    }


    public override void RemoveAuras()
    {
        this.TemporaryStatModify(ModifiableStats.CritChance, -critBuff);
        base.RemoveAuras(); // Clears the list
    }

    public override void ApplyAuras()
    {
        this.TemporaryStatModify(ModifiableStats.CritChance, critBuff);
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"Has [c_crit]{critBuff}[/c] [CRIT] when [c_shield]shielded[/c]. When this attacks, [c_shield]shield[/c] this for the [c_attack]damage[/c] dealt.");
    }
}
