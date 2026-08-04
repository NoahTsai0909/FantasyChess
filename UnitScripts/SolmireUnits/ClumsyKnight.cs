using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ClumsyKnight : UnitInstance
{
    private int shieldBuff;
    public override void InitializeFromSaveData(UnitSaveData data)
    {
        base.InitializeFromSaveData(data);
        shieldBuff = findShieldBuff(CurrentRarity);
    }

    public override void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        base.InitializeEnemy(def, rarity);
        shieldBuff = findShieldBuff(rarity);
    }

    private int findShieldBuff(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 5,
            Rarity.Uncommon => 10,
            Rarity.Rare => 20,
            Rarity.Epic => 40,
            _ => 5
        };
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
                reason = "Clumsy Knight Attack",
                isCrit = abilityCrit
            }
        );

    }

    public override void CombatStartEffect()
    {
        List<UnitInstance> allies = FindAdjacentAllies();
        CombatManager.Instance.ExecuteAction(
            new CombatAction
            {
                type = CombatActionType.Shield,
                source = this,
                target = this,
                amount = shieldBuff * allies.Count,
                reason = "Clumsy Knight Shield"
            }
        );
    }

    public override string GetActiveDescription()
    {
        return ($"[c_attack]Attack[/c] the nearest enemy for [ATK] {stats.Attack}.");
    }

    public override string GetPassiveDescription()
    {
        return ($"Combat Start: [c_shield]shield[/c] this for [SHIELD] {shieldBuff} for each adjacent ally.");
    }

}
