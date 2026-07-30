using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Midas : UnitInstance
{

    public override void CombatStartEffect()
    {
        // Find all enemies with max health <= this unit's max health
        List<UnitInstance> vulnerableEnemies = new List<UnitInstance>();

        List<UnitInstance> allEnemies = FindAllEnemies(); // You'll need to implement this

        foreach (var enemy in allEnemies)
        {
            if (enemy.GetMaxHP() <= stats.MaxHP)
            {
                vulnerableEnemies.Add(enemy);
            }
        }

        // Find the one with the highest max health
        if (vulnerableEnemies.Count > 0)
        {
            UnitInstance target = vulnerableEnemies.OrderByDescending(e => e.GetMaxHP()).First();

            // Kill the target
            CombatManager.Instance.ExecuteAction(new CombatAction
            {
                type = CombatActionType.Kill,
                source = this,
                target = target,
                amount = 0,
                reason = "Midas's Golden Touch"
            });
            Debug.Log("Midas 's Golden Touch activated! Killed " + target.name);

            // Gain gold equal to its value
            int goldGained = target.GetCurrentValue();
            RunManager.Instance.Stats.CurrentGold += goldGained;
            Debug.Log($"Gained {goldGained} gold. Total: {RunManager.Instance.Stats.CurrentGold}");
        }
    }

    protected override void UseAbility()
    {

        UnitInstance target = FindNearestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Midas Attack"
            }
        );
        base.UseAbility();
    }
    public override string GetActiveDescription()
    {
        return ($"[c_Attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}.");
    }

    public override string GetPassiveDescription()
    {
        return ("Combat Start: Kill the highest-health enemy with [MAXHEALTH] less than or equal to this unit's [MAXHEALTH]. Gain [GOLD] equal to its value.");
    }
}
