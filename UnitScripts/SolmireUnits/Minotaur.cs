using UnityEngine;

public class Minotaur : UnitInstance
{
    private int bonusMaxHPstat = 2;
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
            Rarity.Common => 1,
            Rarity.Uncommon => 2,
            Rarity.Rare => 3,
            Rarity.Epic => 4,
            _ => 1
        };
    }

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        base.EnterCombat(grid, row, col, isPlayer);

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
        return ($"Attack the nearest enemy for {stats.Attack} damage.\nPassive: When this unit survives combat, gain {bonusMaxHPstat} max hp.");
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
