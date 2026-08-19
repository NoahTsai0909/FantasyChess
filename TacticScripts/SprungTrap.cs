using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SprungTrap : TacticInstance
{
    public int buffValue = 1;
    public bool firstTimeTriggered;
    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 1,
            Rarity.Epic => 2,
            _ => 1
        };
    }

    public override void InitializeFromSaveData(RunManager.TacticSaveData data)
    {
        base.InitializeFromSaveData(data);
        buffValue = GetBuffValue(CurrentRarity);
    }

    protected override void OnTierUpgraded()
    {
        base.OnTierUpgraded();
        buffValue = GetBuffValue(CurrentRarity);
    }

    public override void EnterCombat()
    {
        base.EnterCombat();
        firstTimeTriggered = false; 
        CombatEventBus.OnActionResolved += HandleActionResolved;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    private void HandleActionResolved(CombatAction action)
    {
        if (firstTimeTriggered) return;
        if (action.source == null) return;
        if (action.source.isPlayer == this.isPlayer) return;
        if (enemyGrid == null) return;
        List<UnitInstance> enemies = FindAllEnemies();
        foreach (UnitInstance target in enemies)
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ExecuteAction(new CombatAction
                {
                    type = CombatActionType.ApplySlow,
                    source = null, // Null because Tactics aren't physical units!
                    target = target,
                    amount = buffValue,
                    reason = tacticName
                });
            }
        }
        firstTimeTriggered = true;
    }



    public override string GetDescription()
    {
        return $"When an enemy uses an ability for the first time, [c_slow]slow[/c] all enemies {buffValue}.";
    }
}