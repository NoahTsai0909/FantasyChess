using UnityEngine;

public class PerfectedCrown : UnitInstance, IConsumable
{

    public bool OnConsume(UnitInstance target)
    {
        if (target == null) return false;

        if (target.CurrentRarity == Rarity.Rare)
        {
            target.UpgradeTier();
            return true;
        }
        return false;
    }

    public override string GetActiveDescription()
    {
        return "Consume this to upgrade the tier of a [c_rare]rare[/c] unit.";
    }
}
