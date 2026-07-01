using UnityEngine;
using System.Collections.Generic;

public class Seamstress : UnitInstance
{
    private int maxHealthBuff = 25;
    private UnitInstance frontAlly;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Rare) { 
            maxHealthBuff = 25;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            maxHealthBuff = 50;
        }
        else
        {
            maxHealthBuff = 25;
        }
    }

    public override void CombatStartEffect()
    {
        List<UnitInstance> adjacentAllies = FindAdjacentAllies();
        int expectedCol = isPlayer ? col + 1 : col - 1;

        foreach (UnitInstance adjacentAlly in adjacentAllies)
        {

            if (adjacentAlly.col == expectedCol)
            {
                frontAlly = adjacentAlly;
                Debug.Log($"Seamstress found front ally: {frontAlly.name}");
                frontAlly.TemporaryStatModify(ModifiableStats.MaxHP, (frontAlly.GetCurrentHP() * maxHealthBuff)/100);
                Debug.Log($"Seamstress applied max health buff to {frontAlly.name}: +{maxHealthBuff}% max health.");
            }
               
        }
    }

    /*public override void Die()
    {
        if (frontAlly != null){
            frontAlly.TemporaryStatModify(ModifiableStats.MaxHP, -(frontAlly.GetCurrentHP() * maxHealthBuff)/100);
        }
        base.Die();
    }*/

    protected override void UseAbility()
    {

        UnitInstance target = FindNearestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Seamstress Attack"
            }
        );
        base.UseAbility();
    }
    public override string GetAbilityDescription()
    {
        return ($"Attack the nearest enemy for {stats.Attack} damage. \n Combat Start: The ally in front of this has +{maxHealthBuff}% max health.");
    }
}
