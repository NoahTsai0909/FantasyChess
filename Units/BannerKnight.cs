using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BannerKnight : UnitInstance
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Banner Knight deployed");
    }


    protected override void UseAbility()
    {
        Debug.Log($"{unitName} uses ability!");

        // Use parent's FindNearestEnemy() method
        UnitInstance target = FindNearestEnemy();

        if (target != null)
        {
            target.TakeDamage(stats.Attack);
            Debug.Log($"{unitName} attacks {target.unitName} for 10 damage!");

        }
        else
        {
            Debug.Log("No enemy found to attack!");
        }
        base.UseAbility();
    }

    
}
