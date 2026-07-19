using UnityEngine;

public class Longshot : UnitInstance
{
    private int critModifier = 10;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        critModifier = findBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        critModifier = findBuff(rarity);
    }

    private int findBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 40,
            _ => 10
        };
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindFarthestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
        new CombatAction
        {
            type = CombatActionType.Damage,
            source = this,
            target = target,
            amount = stats.Attack,
            reason = "Longshot Attack",
            isCrit = abilityCrit
            }
        );
        this.TemporaryStatModify(ModifiableStats.CritChance, critModifier);
        
    }
    public override string GetActiveDescription()
    {
        return ($"Attack the farthest enemy for {stats.Attack} damage. Gain {critModifier}% crit chance.");
    }

}
