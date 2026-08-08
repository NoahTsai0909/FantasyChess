using UnityEngine;

public class UnstableCrown : UnitInstance, IConsumable
{

    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;

        if (target.CurrentRarity == Rarity.Common)
        {
            target.UpgradeTier();
            return true;
        }
        return false;
    }

    public override string GetActiveDescription()
    {
        return "Consume this to upgrade the tier of a common unit.";
    }
}

