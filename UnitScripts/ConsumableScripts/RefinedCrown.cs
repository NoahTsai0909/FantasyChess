using UnityEngine;

public class RefinedCrown : UnitInstance, IConsumable
{

    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;

        if (target.CurrentRarity == Rarity.Uncommon)
        {
            target.UpgradeTier();
            return true;
        }
        return false;
    }

    public override string GetActiveDescription()
    {
        return "Consume this to upgrade the tier of an uncommon unit.";
    }
}