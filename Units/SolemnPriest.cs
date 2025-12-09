using UnityEngine;

public class SolemnPriest : UnitInstance
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"{unitName} deployed!");
    }

    protected override void UseAbility()
    {
        Debug.Log($"{unitName} uses ability!");

        // Use parent's FindNearestEnemy() method
        UnitInstance target = FindLowestHealthAlly();

        if (target != null)
        {
            target.HealDamage(healValue);
            Debug.Log($"{unitName} heals {target.unitName} for 8 damage!");

        }
        else
        {
            Debug.Log("No target found to heal!");
        }
    }
}
