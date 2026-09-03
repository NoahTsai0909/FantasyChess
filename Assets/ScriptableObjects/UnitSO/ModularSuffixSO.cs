using UnityEngine;
using System.Collections.Generic;

public enum SuffixTargeting
{
    NearestEnemy,
    FarthestEnemy,
    AllEnemies,
    LowestHealthAlly,
    Self,
    AllAllies,
    RandomAlly,
}

[CreateAssetMenu(menuName = "Unit/Mutations/Modular Suffix")]
public class ModularSuffixSO : MutationSuffixSO
{
    [Header("Suffix Rules")]
    public SuffixTargeting targetSelection;

    private int GetCalculatedAmount(UnitInstance caster, ModifiableStats stat)
    {
        bool isAoE = targetSelection == SuffixTargeting.AllEnemies || targetSelection == SuffixTargeting.AllAllies;
        if (stat == ModifiableStats.Slow || stat == ModifiableStats.Haste)
        {
            return isAoE ? 1 : 3;
        }
        int baseAmount = 0;
        if (stat == ModifiableStats.Burn) baseAmount = caster.Stats.Burn;
        else if (stat == ModifiableStats.Poison) baseAmount = caster.Stats.Poison;
        else if (stat == ModifiableStats.Attack) baseAmount = caster.Stats.Attack;
        else if (stat == ModifiableStats.Heal) baseAmount = caster.Stats.Heal;
        else if (stat == ModifiableStats.Shield) baseAmount = caster.Stats.Shield;

        return isAoE ? Mathf.Max(1, baseAmount / 5) : baseAmount;
    }

    public override void ExecuteEffect(UnitInstance caster)
    {
        List<UnitInstance> target = new List<UnitInstance>(); 
        switch (targetSelection)
        {
            case SuffixTargeting.NearestEnemy:
                var nearest = caster.FindNearestEnemy();
                if (nearest != null) target.Add(nearest);
                break;
            case SuffixTargeting.FarthestEnemy:
                var farthest = caster.FindFarthestEnemy();
                if (farthest != null) target.Add(farthest);
                break;
            case SuffixTargeting.AllEnemies:
                target = caster.FindAllEnemies();
                break;
            case SuffixTargeting.LowestHealthAlly:
                var lowestAlly = caster.FindLowestHealthAlly();
                if (lowestAlly != null) target.Add(lowestAlly);
                break;
            case SuffixTargeting.Self:
                target.Add(caster);
                break;
            case SuffixTargeting.AllAllies:
                target = caster.FindAllAllies();
                break;
            case SuffixTargeting.RandomAlly:
                var random = caster.FindRandomAlly();
                if (random != null) target.Add(random);
                break;
        }

        if (target.Count == 0 || caster.currentPrefix == null) return;

        ModifiableStats grantedStat = caster.currentPrefix.statToGrant;


        int finalAmount = GetCalculatedAmount(caster, grantedStat);

        if (grantedStat == ModifiableStats.Burn)
        {
            foreach (var t in target)
            {
                if (CombatManager.Instance != null && t != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.ApplyBurn,
                        source = caster,
                        target = t,
                        amount = finalAmount,
                        reason = caster.currentPrefix.name
                    });
                }
            }
        }
        else if (grantedStat == ModifiableStats.Poison)
        {
            foreach (var t in target)
            {
                if (CombatManager.Instance != null && t != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.ApplyPoison,
                        source = caster,
                        target = t,
                        amount = finalAmount,
                        reason = caster.currentPrefix.name
                    });
                }
            }
        }
        else if (grantedStat == ModifiableStats.Attack)
        {
            foreach (var t in target)
            {
                if (CombatManager.Instance != null && t != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.Damage,
                        source = caster,
                        target = t,
                        amount = finalAmount,
                        reason = caster.currentPrefix.name
                    });
                }
            }
        }
        else if (grantedStat == ModifiableStats.Heal)
        {
            foreach (var t in target)
            {
                if (CombatManager.Instance != null && t != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.Heal,
                        source = caster,
                        target = t,
                        amount = finalAmount,
                        reason = caster.currentPrefix.name
                    });
                }
            }
        }
        else if (grantedStat == ModifiableStats.Shield)
        {
            foreach (var t in target)
            {
                if (CombatManager.Instance != null && t != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.Shield,
                        source = caster,
                        target = t,
                        amount = finalAmount,
                        reason = caster.currentPrefix.name
                    });
                }
            }
        }
        else if (grantedStat == ModifiableStats.Slow)
        {
            foreach (var t in target)
            {
                if (CombatManager.Instance != null && t != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.ApplySlow,
                        source = caster,
                        target = t,
                        amount = finalAmount,
                        reason = caster.currentPrefix.name
                    });
                }
            }
        }
        else if (grantedStat == ModifiableStats.Haste)
        {
            foreach (var t in target)
            {
                if (CombatManager.Instance != null && t != null)
                {
                    CombatManager.Instance.ExecuteAction(new CombatAction
                    {
                        type = CombatActionType.ApplyHaste,
                        source = caster,
                        target = t,
                        amount = finalAmount,
                        reason = caster.currentPrefix.name
                    });
                }
            }
        }
        }

    public override string GetActionPhrase(UnitInstance caster, bool capitalizeFirstLetter)
    {
        if (caster.currentPrefix == null) return "";

        string targetString = targetSelection switch
        {
            SuffixTargeting.NearestEnemy => "the nearest enemy",
            SuffixTargeting.FarthestEnemy => "the farthest enemy",
            SuffixTargeting.AllEnemies => "all enemies",
            SuffixTargeting.LowestHealthAlly => "the lowest health ally",
            SuffixTargeting.Self => "this",
            SuffixTargeting.AllAllies => "all allies",
            SuffixTargeting.RandomAlly => "a random ally",
            _ => "an enemy"
        };

        ModifiableStats grantedStat = caster.currentPrefix.statToGrant;

        int finalAmount = GetCalculatedAmount(caster, grantedStat);

        if (grantedStat == ModifiableStats.Burn)
            return $"[c_burn]{(capitalizeFirstLetter ? "B" : "b")}urn[/c] {targetString} for [BURN] {finalAmount}";
        else if (grantedStat == ModifiableStats.Poison)
            return $"[c_poison]{(capitalizeFirstLetter ? "P" : "p")}oison[/c] {targetString} for [POISON] {finalAmount}";
        else if (grantedStat == ModifiableStats.Attack)
            return $"[c_attack]{(capitalizeFirstLetter ? "A" : "a")}ttack[/c] {targetString} for [ATK] {finalAmount}";
        else if (grantedStat == ModifiableStats.Heal)
            return $"[c_heal]{(capitalizeFirstLetter ? "H" : "h")}eal[/c] {targetString} for [HEAL] {finalAmount}";
        else if (grantedStat == ModifiableStats.Shield)
            return $"[c_shield]{(capitalizeFirstLetter ? "S" : "s")}hield[/c] {targetString} for [SHIELD] {finalAmount}";
        else if (grantedStat == ModifiableStats.Slow)
            return $"[c_slow]{(capitalizeFirstLetter ? "S" : "s")}low[/c] {targetString} for [SLOW] {finalAmount}";
        else if (grantedStat == ModifiableStats.Haste)
            return $"[c_haste]{(capitalizeFirstLetter ? "H" : "h")}aste[/c] {targetString} for [HASTE] {finalAmount}";

        return "";
    }
}