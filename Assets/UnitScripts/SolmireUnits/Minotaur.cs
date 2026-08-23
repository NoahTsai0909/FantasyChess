using UnityEngine;

public class Minotaur : UnitInstance
{
    private int bonusMaxHPstat = 5;
    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        bonusMaxHPstat = findHPstat(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        bonusMaxHPstat = findHPstat(rarity);
    }

    private int findHPstat(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 5,
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 40,
            _ => 5
        };
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        bonusMaxHPstat = findHPstat(CurrentRarity);
    }

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);

        CombatEventBus.OnCombatEnd += HandleCombatEnd;
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        // Use parent's FindNearestEnemy() method
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
                    reason = "Minotaur attack",
                    isCrit = abilityCrit
                }
            );

        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"When this unit survives combat, gain [MAXHEALTH] [c_maxhealth]{bonusMaxHPstat}[/c] permanently.");
    }

    private void OnDestroy() { 
        CombatEventBus.OnCombatEnd -= HandleCombatEnd;
    }

    private void HandleCombatEnd()
    {
        RunManager.Instance.GetPermanentStatsForUnit(id).bonusMaxHP += bonusMaxHPstat;
        Debug.Log($"{unitName} uses ability! Max HP: {stats.MaxHP}");
    }
}
