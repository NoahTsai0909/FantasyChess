using UnityEngine;

public class Farmer : UnitInstance
{
    private int goldReward = 1;
    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        goldReward = findGoldReward(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        goldReward = findGoldReward(rarity);
    }

    private int findGoldReward(Rarity rarity)
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
                    reason = "Farmer Attack",
                    isCrit = abilityCrit
                }
            );
            Debug.Log($"{unitName} attacks {target.unitName} for {stats.Attack} damage!");

        }
        else
        {
            Debug.Log("No target found to attack!");
        }
    }

    public override string GetActiveDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage.\nPassive: When this unit survives combat, +{goldReward} gold.");
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= HandleCombatEvent;
    }

    private void HandleCombatEnd()
    {
        if (this.isPlayer)
        {
            RunManager.Instance.Stats.CurrentGold += goldReward;
        }
    }


}
