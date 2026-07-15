using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class FaerieFlare : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        if (stats.Burn >= 20)
        {
            this.TemporaryStatModify(ModifiableStats.Multicast, 1);
        }
        UnitInstance target = FindNearestEnemy();

        if (target == null)
        {
            return;
        }

        CombatManager.Instance.ExecuteAction(
        new CombatAction
        {
            type = CombatActionType.ApplyBurn,
            source = this,
            target = target,
            amount = stats.Burn,
            reason = "Faerie Flare Burn",
            isCrit = abilityCrit
        }
        );
    }

    public override string GetAbilityDescription()
    {
        return ($"If this has at least 20 burn, gain 1 multicast. Burn the nearest enemy for {stats.Burn}.");
    }
}
