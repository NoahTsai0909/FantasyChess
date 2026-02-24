using UnityEngine;

public class Swiftswipe : UnitInstance
{
    private int attackModifier = 2;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Uncommon)
        {
            attackModifier = 2;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            attackModifier = 4;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            attackModifier = 6;
        }
        else
        {
            attackModifier = 2;
        }
    }

    protected override void UseAbility()
    {
        UnitInstance target = FindNearestEnemy();

        if (target != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Damage,
                    source = this,
                    target = target,
                    amount = stats.Attack,
                    reason = "Swiftswipe Attack"
                }
            );
        }

        TemporaryStatModify(ModifiableStats.Attack, attackModifier);
        base.UseAbility();
    }

    public override string GetAbilityDescription()
    {
        return ($"Attacks the nearest enemy for {stats.Attack} damage, then gains {attackModifier} attack.");
    }
}
