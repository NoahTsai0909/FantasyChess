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
            target.HealDamage(stats.Heal);
            Debug.Log($"{unitName} heals {target.unitName} for {stats.Heal} damage!");

        }
        else
        {
            Debug.Log("No target found to heal!");
        }
    }

    public override string GetAbilityDescription()
    {
        stats = RunManager.Instance.GetPreviewStats(Definition);
        return ($"Heal the lowest health ally for {stats.Heal} health.");
    }
}
