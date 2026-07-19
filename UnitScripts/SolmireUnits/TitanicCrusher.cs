using System.Collections.Generic;
using UnityEngine;

public class TitanicCrusher : UnitInstance
{
    List<UnitInstance> enemies = new List<UnitInstance>();
    protected override void UseAbility()
    {
        base.UseAbility();
        enemies = FindAllEnemies();
        foreach (UnitInstance enemy in enemies)
        {
            if (enemy != null && enemy != this)
            {
                CombatManager.Instance.ExecuteAction(
                    new CombatAction
                    {
                        type = CombatActionType.Damage,
                        source = this,
                        target = enemy,
                        amount = stats.MaxHP,
                        reason = "Titanic Crusher Attack",
                        isCrit = abilityCrit
                    }
                );
            }
        }

    }
    public override string GetActiveDescription()
    {
        return ($"Damage all enemies equal to this unit's max health.");
    }
}