using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CinderResurgent : UnitInstance
{
    List<UnitInstance> enemies = new List<UnitInstance>();
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
                reason = "Cinder Resurgent Attack"
            }
        );

        enemies = FindAllEnemies();
        foreach (UnitInstance enemy in enemies)
        {
            if (enemy != null && enemy != this)
            {
                CombatManager.Instance.ExecuteAction(
                    new CombatAction
                    {
                        type = CombatActionType.ApplyBurn,
                        source = this,
                        target = enemy,
                        amount = stats.Burn,
                        reason = "Cinder Resurgent Burn"
                    }
                );
            }
        }

        base.UseAbility();
    }
    public override string GetAbilityDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage. Burn all enemies for {stats.Burn}. When this dies, summon a Scattered Pyre.");
    }

    protected override void OnDeathEffect()
    {
        if (Definition.spawnDefinition != null)
        {
            UnitSpawner.Instance.SpawnUnit(Definition.spawnDefinition, row, col, isPlayer, this, CurrentRarity);
            Debug.Log($"Cinder Resurgent has died and summoned a Scattered Pyre at ({row},{col})");
        }
    }
}
