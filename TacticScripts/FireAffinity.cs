using System.Collections.Generic;
using UnityEngine;

public class FireAffinity : TacticInstance
{
    public int burnBuff = 1;

    public override void ApplyPassiveEffect()
    {
        base.ApplyPassiveEffect();
        if (allyGrid == null) return;

        auraTargets = FindAllAllies();

        if (auraTargets == null) return;

        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.Burn, burnBuff);
            }
        }
    }

    public override void RemovePassiveEffect()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.Burn, -burnBuff);
            }
        }
        base.RemovePassiveEffect();

    }
}
