using UnityEngine;

public class BloodRush : TacticInstance
{
    public int buffValue = 10;

    private int GetBuffValue(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 40,
            _ => 10
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

        if (action.source.isPlayer != this.isPlayer) return;
        if (allyGrid == null) return;
        if (action.isCrit)
        {
            auraTargets = FindAllAllies();

            if (auraTargets == null) return;

            foreach (UnitInstance target in auraTargets)
            {
                if (target != null)
                {
                    target.TemporaryStatModify(ModifiableStats.Attack, buffValue);
                }
            }
        }
    }



    public override string GetDescription()
    {
        return $"When an ally [c_crit]crits[/c], all allies get [ATK]{buffValue}.";
    }
}

