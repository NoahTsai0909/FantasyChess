using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Combustion : TacticInstance
{
    public int buffValue = 3;
    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => 3,
            Rarity.Epic => 6,
            _ => 3
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
        CombatEventBus.OnCombatEvent += OnCombatEvent;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= OnCombatEvent;
    }

    private void OnCombatEvent(CombatEventBus.CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        if (type == CombatEventBus.CombatEventType.UnitDied && source.isPlayer != this.isPlayer && enemyGrid != null)
        {
            List<UnitInstance> targets = FindAllEnemies();
            if (targets.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, targets.Count);
                UnitInstance enemy = targets[randomIndex];
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.ApplyBurn,
                        source = null, // Null because Tactics aren't physical units!
                        target = enemy,
                        amount = buffValue,
                        reason = tacticName
                    });
                }
            }
        }
    }



    public override string GetDescription()
    {
        return $"When an enemy dies, [c_burn]burn[/c] a random enemy for [BURN] {buffValue}.";
    }
}