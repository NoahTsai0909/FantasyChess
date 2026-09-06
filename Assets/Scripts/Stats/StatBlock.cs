using UnityEngine;

[System.Serializable]
public enum ModifiableStats
{
    None,
    Attack,
    Heal,
    MaxHP,
    Cooldown,
    Shield,
    Burn,
    Poison,
    Slow,
    Haste,
    MaxEnergy,
    Multicast,
    Value,
    CritChance,
    Tags,
}

public class StatBlock
{
    // These are now true snapshot variables. We calculate them ONCE in the constructor.
    public int Attack { get; private set; }
    public int Heal { get; private set; }
    public int MaxHP { get; private set; }
    public int Shield { get; private set; }
    public int Burn { get; private set; }
    public int Poison { get; private set; }
    public int maxEnergy { get; private set; }
    public int Slow { get; private set; }
    public int Haste { get; private set; }
    public int Multicast { get; private set; }
    public int Value { get; private set; }
    public int CritChance { get; private set; }
    public float Cooldown { get; private set; }
    public UnitTagFlags Tags { get; private set; }

    public StatBlock(IStatSource def, PermanentStats perm, TemporaryStats temp, MutationPrefixSO prefix, ModifiableStats mainStat)
    {
        // 1. Calculate Foundation (Base + Perm + Temp) safely handling nulls
        Attack = def.Attack + (perm != null ? perm.bonusAttack : 0) + (temp != null ? temp.attackBonus : 0);
        Heal = def.Heal + (perm != null ? perm.bonusHeal : 0) + (temp != null ? temp.healBonus : 0);
        MaxHP = def.MaxHP + (perm != null ? perm.bonusMaxHP : 0) + (temp != null ? temp.maxHPBonus : 0);
        Shield = def.Shield + (perm != null ? perm.bonusShield : 0) + (temp != null ? temp.shieldBonus : 0);
        Burn = def.Burn + (perm != null ? perm.bonusBurn : 0) + (temp != null ? temp.burnBonus : 0);
        Poison = def.Poison + (perm != null ? perm.bonusPoison : 0) + (temp != null ? temp.poisonBonus : 0);
        maxEnergy = def.MaxEnergy + (perm != null ? perm.bonusMaxEnergy : 0) + (temp != null ? temp.maxEnergyBonus : 0);
        Slow = def.Slow + (perm != null ? perm.bonusSlow : 0) + (temp != null ? temp.slowBonus : 0);
        Haste = def.Haste + (perm != null ? perm.bonusHaste : 0) + (temp != null ? temp.hasteBonus : 0);
        Multicast = def.Multicast + (perm != null ? perm.bonusMulticast : 0) + (temp != null ? temp.multicastBonus : 0);
        Value = def.Value + (perm != null ? perm.bonusValue : 0) + (temp != null ? temp.valueBonus : 0);
        CritChance = def.CritChance + (perm != null ? perm.bonusCritChance : 0) + (temp != null ? temp.critChanceBonus : 0);

        UnitTagFlags baseTags = def.TagFlags;
        UnitTagFlags permTags = perm != null ? perm.bonusTags : 0;
        UnitTagFlags tempTags = temp != null ? temp.tagBonus : 0;

        Tags = baseTags | permTags | tempTags;

        // Cooldown calculation
        float permCd = perm != null ? perm.cooldownReduction : 0f;
        float tempCd = temp != null ? temp.cooldownDelta : 0f;
        float reductionPercentage = Mathf.Clamp((permCd + tempCd) / 100f, 0f, 0.90f);
        float reducedCooldown = def.Cooldown * (1f - reductionPercentage);
        Cooldown = Mathf.Max(0.5f, reducedCooldown);

        if (prefix != null)
        {
            if (mainStat == ModifiableStats.None)
            {
                AddStat(prefix.statToGrant, prefix.flatBonusAmount);
            }
            else
            {
                int mainStatValue = GetStatValue(mainStat);

                if (prefix.statToGrant == mainStat)
                {
                    int matchingBonus = Mathf.FloorToInt(mainStatValue * 0.5f);
                    AddStat(prefix.statToGrant, matchingBonus);
                }
                else
                {
                    int mutationBonus = CalculateMutationMath(mainStatValue, mainStat, prefix.statToGrant);
                    AddStat(prefix.statToGrant, mutationBonus);
                }
            }
            if (prefix.statToGrant == ModifiableStats.Burn) Tags |= UnitTagFlags.Burn;
            else if (prefix.statToGrant == ModifiableStats.Poison) Tags |= UnitTagFlags.Poison;
            else if (prefix.statToGrant == ModifiableStats.Heal) Tags |= UnitTagFlags.Heal;
            else if (prefix.statToGrant == ModifiableStats.Shield) Tags |= UnitTagFlags.Shield;
            else if (prefix.statToGrant == ModifiableStats.Attack) Tags |= UnitTagFlags.Damage;
            else if (prefix.statToGrant == ModifiableStats.MaxHP) Tags |= UnitTagFlags.MaxHP;
            else if (prefix.statToGrant == ModifiableStats.Slow) Tags |= UnitTagFlags.Slow;
            else if (prefix.statToGrant == ModifiableStats.Haste) Tags |= UnitTagFlags.Haste;
        }
    }

    /* =========================
     * MUTATION HELPERS
     * ========================= */

    private int GetStatValue(ModifiableStats stat)
    {
        return stat switch
        {
            ModifiableStats.Attack => Attack,
            ModifiableStats.Heal => Heal,
            ModifiableStats.Shield => Shield,
            ModifiableStats.Burn => Burn,
            ModifiableStats.Poison => Poison,
            ModifiableStats.MaxHP => MaxHP,
            _ => 0
        };
    }

    private void AddStat(ModifiableStats stat, int amount)
    {
        switch (stat)
        {
            case ModifiableStats.Attack: Attack += amount; break;
            case ModifiableStats.Heal: Heal += amount; break;
            case ModifiableStats.Shield: Shield += amount; break;
            case ModifiableStats.Burn: Burn += amount; break;
            case ModifiableStats.Poison: Poison += amount; break;
            case ModifiableStats.MaxHP: MaxHP += amount; break;
        }
    }

    private int CalculateMutationMath(int mainValue, ModifiableStats mainStat, ModifiableStats grantedStat)
    {
        float mainWeight = GetStatWeight(mainStat);
        float grantedWeight = GetStatWeight(grantedStat);

        // e.g., 100 Attack (Weight 1) converting to Burn (Weight 5) = 100 * (1/5) = 20.
        return Mathf.FloorToInt(mainValue * (mainWeight / grantedWeight));
    }

    public float GetStatWeight(ModifiableStats stat)
    {
        return stat switch
        {
            ModifiableStats.Burn => 5f,
            ModifiableStats.Poison => 5f,
            ModifiableStats.Attack => 1f,
            ModifiableStats.Heal => 1f,
            ModifiableStats.Shield => 1f,
            ModifiableStats.MaxHP => 0.1f,
            _ => 1f
        };
    }
}