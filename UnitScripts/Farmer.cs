using UnityEngine;

public class Farmer : UnitInstance
{
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
                    reason = "Farmer Attack"
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
        return ($"Attack the nearest enemy for {stats.Attack} damage.\nPassive: When this unit survives combat, +1 gold.");
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= HandleCombatEvent;
    }

    private void HandleCombatEnd()
    {
        RunManager.Instance.Stats.CurrentGold+=1;
        Debug.Log("Farmer just farmed 1 gold!");
    }


}
