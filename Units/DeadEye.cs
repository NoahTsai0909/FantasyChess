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
        Debug.Log($"{unitName} uses ability!");
        UnitInstance target = FindFarthestEnemy();

        if (target != null)
        {
            target.TakeDamage(attackValue);
            Debug.Log($"{unitName} attacks {target.unitName} for {attackValue}!");

        }
        else
        {
            Debug.Log("No enemy found to attack!");
        }
        base.UseAbility();
    }
}
