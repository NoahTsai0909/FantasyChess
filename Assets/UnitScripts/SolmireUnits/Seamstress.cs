using UnityEngine;
using System.Collections.Generic;

public class Seamstress : UnitInstance
{
    private int maxHealthBuffPercent = 25;
    private UnitInstance frontAlly;
    private bool buffApplied = false;
    private int buffAmount = 0;
    private int originalMaxHP = 0;

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
            Rarity.Rare => 25,
            Rarity.Epic => 50,
            _ => 25
        };
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
                ApplyBuff();
                break;
            }
               
        }
    }

    private void ApplyBuff()
    {
        if (frontAlly == null || buffApplied) return;

        // Store original max HP BEFORE applying buff
        originalMaxHP = frontAlly.GetCurrentHP();

        // Calculate and apply buff
        buffAmount = (originalMaxHP * maxHealthBuffPercent) / 100;
        frontAlly.TemporaryStatModify(ModifiableStats.MaxHP, buffAmount);

        buffApplied = true;

        Debug.Log($"Applied +{maxHealthBuffPercent}% max HP to {frontAlly.unitName}. +{buffAmount} HP");
    }

    public override void Die()
    {
        if (frontAlly != null && buffApplied && frontAlly.gameObject != null)
        {
            RemoveBuff();
        }
        base.Die();
    }

    private void RemoveBuff()
    {
        if (frontAlly == null) return;

        // Calculate the current max HP without the buff
        int currentMaxHP = frontAlly.GetMaxHP();
        int unbuffedMaxHP = currentMaxHP - buffAmount;

        // Option A: Proportional scaling (most common in games)
        // Scale current HP proportionally to the new max
        float healthPercent = (float)frontAlly.GetCurrentHP() / currentMaxHP;
        int newCurrentHP = Mathf.RoundToInt(unbuffedMaxHP * healthPercent);
        int newMaxHP = unbuffedMaxHP;

        // Apply the changes manually since we need to adjust both current and max
        // We can't just use TemporaryStatModify with negative buff
        // because that would only adjust max, not current

        // Direct manipulation (you'll need to add these methods or properties)
        // This assumes you have a way to set HP directly
        frontAlly.SetMaxHP(newMaxHP);
        frontAlly.SetCurrentHP(Mathf.Min(newCurrentHP, newMaxHP));

        buffApplied = false;

        Debug.Log($"Removed buff from {frontAlly.unitName}. New max: {newMaxHP}, New current: {frontAlly.GetCurrentHP()}");
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        UnitInstance target = FindNearestEnemy();
        if (target == null) return;

        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Damage,
                source = this,
                target = target,
                amount = stats.Attack,
                reason = "Seamstress Attack",
                isCrit = abilityCrit
            }
        );
    }
    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"Combat Start: The ally in front of this has [c_maxhealth]+{maxHealthBuffPercent}%[/c] [MAXHEALTH].");
    }
}
