using UnityEngine;

public class Ranger : UnitInstance
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Ranger deployed");
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindFarthestEnemy();

        if (target != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = stats.Attack,
                    reason = "Ranger attack",
                    isCrit = abilityCrit
                }
            );
            Debug.Log($"{unitName} attacks {target.unitName} for {stats.Attack}!");

        }
        else
        {
            Debug.Log("No enemy found to attack!");
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the farthest enemy for [ATK] {stats.Attack}.");
    }
}
