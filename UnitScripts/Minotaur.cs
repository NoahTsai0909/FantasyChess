using UnityEngine;

public class Minotaur : UnitInstance
{
    private int bonusMaxHPstat = 2;


    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        base.EnterCombat(grid, row, col, isPlayer);

        CombatEventBus.OnCombatEnd += HandleCombatEnd;
    }

    protected override void UseAbility()
    {

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
                    reason = "Minotaur attack"
                }
            );
            Debug.Log($"{unitName} attacks {target.unitName} for {stats.Attack} damage!");

        }
        else
        {
            Debug.Log("No target found to attack!");
        }
        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        stats = RunManager.Instance.GetPreviewStats(Definition, CurrentRarity);
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
