using UnityEngine;

public class Swiftswipe : UnitInstance
{
    private int attackModifier = 5;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        attackModifier = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        attackModifier = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 5,
            Rarity.Rare => 10,
            Rarity.Epic => 20,
            _ => 5
        };
    }

    protected override void UseAbility()
    {
        base.UseAbility();
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
                    reason = "Swiftswipe Attack",
                    isCrit = abilityCrit
                }
            );
        }

        TemporaryStatModify(ModifiableStats.Attack, attackModifier);
    }

    public override string GetActiveDescription()
    {
        return ($"Attacks the nearest enemy for {stats.Attack} damage, then gains {attackModifier} attack.");
    }
}
