using UnityEngine;

public class SolemnPriest : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        // Use parent's FindNearestEnemy() method
        UnitInstance target = FindLowestHealthAlly();

        if (target != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Heal,
                    source = this,
                    target = target,
                    amount = stats.Heal,
                    reason = "SolemnPriest heal",
                    isCrit = abilityCrit
                }
            );
            Debug.Log($"{unitName} heals {target.unitName} for {stats.Heal} damage!");

        }
        else
        {
            Debug.Log("No target found to heal!");
        }
    }

    public override string GetActiveDescription()
    {
        return ($"Heal the lowest health ally for {stats.Heal} health.");
    }
}
