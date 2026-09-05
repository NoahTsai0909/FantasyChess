using Unity.VisualScripting;
using UnityEngine;

public class ShiftingSigil : UnitInstance, IConsumable
{


    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;

        TransformParams myRules = new TransformParams
        {
            rarityRule = TransformRule.Same,
            regionRule = TransformRule.Same,
            provisionRule = TransformRule.Same,
            keepMutations = true
        };

        PlayerUnitManager.Instance.TransformUnit(target, myRules);
        return true;
    }

    public override string GetActiveDescription()
    {
        return ($"Consume this to transform a unit permanently.");
    }
}

