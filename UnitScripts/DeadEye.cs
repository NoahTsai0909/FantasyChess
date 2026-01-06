using UnityEngine;

public class DeadEye : UnitInstance
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Deadeye deployed");
    }

    protected override void UseAbility()
    {
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
                    reason = "Deadeye attack"
                }
            );
            Debug.Log($"{unitName} attacks {target.unitName} for {stats.Attack}!");

        }
        else
        {
            Debug.Log("No enemy found to attack!");
        }
        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        stats = RunManager.Instance.GetPreviewStats(Definition, CurrentRarity);
        return ($"Attack the farthest enemy for {stats.Attack} damage.");
    }
}
