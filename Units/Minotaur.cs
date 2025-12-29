using UnityEngine;

public class Minotaur : UnitInstance
{
    private int bonusMaxHPstat = 2;
    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"{unitName} deployed!");
    }

    protected override void UseAbility()
    {
        Debug.Log($"{unitName} uses ability! Max HP: {stats.MaxHP}");

        // Use parent's FindNearestEnemy() method
        UnitInstance target = FindNearestEnemy();

        if (target != null)
        {
            target.TakeDamage(stats.Attack);
            Debug.Log($"{unitName} attacks {target.unitName} for {stats.Attack} damage!");

        }
        else
        {
            Debug.Log("No target found to attack!");
        }
    }

    public override string GetAbilityDescription()
    {
        stats = RunManager.Instance.GetPreviewStats(Definition, CurrentRarity);
        return ($"Attack the nearest enemy for {stats.Attack} damage.\nPassive: When this unit survives combat, gain {bonusMaxHPstat} max hp.");
    }

    private void OnEnable()
    {
        CombatEventBus.OnCombatEnd += HandleCombatEnd;
    }

    private void OnDisable()
    {
        CombatEventBus.OnCombatEnd -= HandleCombatEnd;
    }

    private void HandleCombatEnd()
    {
        RunManager.Instance.GetPermanentStatsForUnit(id).bonusMaxHP += bonusMaxHPstat;
    }
}
