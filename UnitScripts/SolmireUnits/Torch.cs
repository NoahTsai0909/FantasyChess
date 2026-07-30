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
        burnBuff = findBurnBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        burnBuff = findBurnBuff(rarity);
    }

    private int findBurnBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 1,
            Rarity.Uncommon => 2,
            Rarity.Rare => 4,
            Rarity.Epic => 8,
            _ => 1
        };
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

    public override string GetPassiveDescription()
    {
        return ($"Combat Start: Adjacent allies have [c_burn]+{burnBuff}[/c] [BURN].");
    }

}
