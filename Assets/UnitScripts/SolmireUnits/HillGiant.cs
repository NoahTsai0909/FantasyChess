using UnityEngine;

public class HillGiant :UnitInstance
{
    private int maxHealthBuffPercent = 5;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        maxHealthBuffPercent = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        maxHealthBuffPercent = findBuff(rarity);
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

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        maxHealthBuffPercent = findBuff(CurrentRarity);
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
                    reason = "HillGiant attack",
                    isCrit = abilityCrit
                }
            );
        }
        this.TemporaryStatModify(ModifiableStats.Attack, GetMaxHP() * maxHealthBuffPercent / 100);
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}. Gain [ATK] equal to {maxHealthBuffPercent}% of [MAXHEALTH] ({GetMaxHP() * maxHealthBuffPercent / 100}).");
    }
}
