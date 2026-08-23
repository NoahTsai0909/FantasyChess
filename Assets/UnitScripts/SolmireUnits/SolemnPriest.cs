using UnityEngine;

public class SolemnPriest : UnitInstance
{
    private int maxhealthBuff;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        maxhealthBuff = findShieldBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        maxhealthBuff = findShieldBuff(rarity);
    }

    private int findShieldBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 5,
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 40,
            _ => 5
        };
    }
    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindLowestHealthAlly();

        if (target != null)
        {
            CombatManager.Instance.ExecuteAction(
                new CombatAction
                {
                    type = CombatActionType.Heal,
                    source = this,
                    target = target,
                    amount = stats.Heal,
                    reason = "SolemnPriest heal",
                    isCrit = abilityCrit
                }
            );
            target.TemporaryStatModify(ModifiableStats.MaxHP, maxhealthBuff);

        }
        else
        {
            Debug.Log("No target found to heal!");
        }
    }

    public override string GetActiveDescription()
    {
        return ($"[c_heal]Heal[/c] the lowest health ally for [HEAL] {stats.Heal}. It gains [c_maxhealth]{maxhealthBuff}[/c] [MAXHEALTH].");
    }
}
