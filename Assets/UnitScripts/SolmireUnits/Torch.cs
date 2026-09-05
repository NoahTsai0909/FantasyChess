using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static CombatEventBus;

public class Torch : UnitInstance
{
    private int burnBuff;
    private int mutationTriggerCount = 0;
    private int mutationTriggerThreshold = 3; 

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

    public override string GetMutationTriggerText()
    {
        return ($"<br>Every {mutationTriggerThreshold} times an [c_adjacent]adjacent[/c] ally uses an ability, ");
    }
    public override string GetPassiveDescription()
    {
        return ($"[c_adjacent]Adjacent[/c] allies have [c_burn]+{burnBuff}[/c] [BURN].");
    }

    public override void RemoveAuras()
    {
        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.Burn, -burnBuff);
            }
        }
        base.RemoveAuras(); // Clears the list
    }

    public override void ApplyAuras()
    {

        if (myGrid == null) return;

        auraTargets = FindAdjacentAllies();

        if (auraTargets == null) return;

        foreach (UnitInstance target in auraTargets)
        {
            if (target != null)
            {
                target.TemporaryStatModify(ModifiableStats.Burn, burnBuff);
            }
        }
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        burnBuff = findBurnBuff(CurrentRarity);
    }

    public override void EnterCombat(GridManager grid, int row, int col, bool isPlayer, bool startCombat = true)
    {
        base.EnterCombat(grid, row, col, isPlayer, startCombat);

        CombatEventBus.OnCombatEvent += HandleCombatEvent;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= HandleCombatEvent;
    }

    protected override void HandleCombatEvent(CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        if (!inCombat || currentSuffix == null) return;
        if (type != CombatEventType.AbilityUsed) return;
        if (source.isPlayer != this.isPlayer) return;
        if (auraTargets?.Contains(source) == false) return;
        mutationTriggerCount++;
        if ( mutationTriggerCount == mutationTriggerThreshold)
        {
            mutationTriggerCount = 0;
            currentSuffix.ExecuteEffect(this);
        }
    }

}
