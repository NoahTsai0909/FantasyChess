using UnityEngine;
using System;



[System.Serializable]


public class UnitSaveData
{
    public Guid id;
    public UnitDefinition definition;
    public Rarity rarity;
    public int provisionCost;

    public int provisionModifier;
    public int valueModifier;

    public int EffectiveProvision => Mathf.Max(0, definition.provisionCost + provisionModifier);

    public int BaseValue => RarityToMultiplier(rarity) * EffectiveProvision;
    public int EffectiveValue => Mathf.Max(0, BaseValue + valueModifier);

    public UnitSaveData()
    {
        id = Guid.NewGuid();
    }

    public static int RarityToMultiplier(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 1;
            case Rarity.Uncommon: return 2;
            case Rarity.Rare: return 3;
            case Rarity.Epic: return 4;
            default: return 1;
        }
    }
}

