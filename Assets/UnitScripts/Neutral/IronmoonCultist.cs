using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;

public class IronmoonCultist : UnitInstance
{
    protected override void UseAbility()
    {
        base.UseAbility();
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Kill,
                source = this,
                target = this,
                amount = 0,
                reason = "Ironmoon Cultist self kill"
            }
        );
    }

    protected override void OnDeathEffect()
    {
        if (myGrid == null) return;
        List<UnitInstance> targets = FindAllAllies();
        foreach (UnitInstance target in targets)
        {
            CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = target,
                amount = stats.Shield,
                reason = "Ironmoon Cultist shield"
            }
        );
        }
    }


    public override string GetActiveDescription()
    {
        return "Kill this.";
    }
    public override string GetPassiveDescription()
    {
        return $"When this dies, [c_shield]shield[/c] all allies for [SHIELD] {stats.Shield}.";
    }
}
