using System.Runtime.CompilerServices;
using UnityEngine;

public class FeatherweightShields : TacticInstance
{
    public int buffValue = 3;
    private int hasteValue = 2;
    private int timesToBuff = 0;

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
        CombatEventBus.OnActionResolved += HandleActionResolved;
    }

    private void OnDestroy()
    {
        CombatEventBus.OnActionResolved -= HandleActionResolved;
    }

    private void HandleActionResolved(CombatAction action)
    {
        if (timesToBuff > buffValue) return;
        if (action.source == null) return;
        if (action.type != CombatActionType.Shield || action.target.isPlayer != this.isPlayer) return;
        if (allyGrid == null) return;
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ExecuteAction(new CombatAction
            {
                type = CombatActionType.ApplyHaste,
                source = null, // Null because Tactics aren't physical units!
                target = action.target,
                amount = hasteValue,
                reason = tacticName
            });
        }
        timesToBuff += 1;
    }



    public override string GetDescription()
    {
        return $"The first {buffValue} times an ally is [c_shield]shielded[/c], [c_haste]haste[/c] it [HASTE]{hasteValue}.";
    }
}
