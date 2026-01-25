using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Torch : UnitInstance
{

    private List<UnitInstance> adjacentAllies;
    private int burnBuff;

    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        if (CurrentRarity == Rarity.Common)
        {
            burnBuff = 1;
        }
        else if (CurrentRarity == Rarity.Uncommon)
        {
            burnBuff = 2;
        }
        else if (CurrentRarity == Rarity.Rare)
        {
            burnBuff = 3;
        }
        else if (CurrentRarity == Rarity.Epic)
        {
            burnBuff = 4;
        }
        else
        {
            burnBuff = 1;
        }
    }
    public override void CombatStartEffect()
    {
        Debug.Log("Torch combat start effect");
        adjacentAllies = FindAdjacentAllies();

        Debug.Log($"Found {adjacentAllies.Count} adjacent allies");

        foreach (UnitInstance adjacentAlly in adjacentAllies)
        {
            
            adjacentAlly.TemporaryStatModify(ModifiableStats.Burn, burnBuff);
        }
    }

    public override void Die()
    {
        Debug.Log($"Torch.Die() called at frame {Time.frameCount}");
        foreach (UnitInstance adjacentAlly in adjacentAllies)
        {
            adjacentAlly.TemporaryStatModify(ModifiableStats.Burn, -burnBuff);
        }
        base.Die();
    }

    public override string GetAbilityDescription()
    {
        return ($"Adjacent allies have +{burnBuff} burn.");
    }

}
