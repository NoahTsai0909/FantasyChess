using UnityEngine;

public class Farmer : UnitInstance
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"{unitName} deployed!");
    }

    protected override void UseAbility()
    {
        Debug.Log($"{unitName} uses ability!");

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
        return ($"Attack the nearest enemy for {stats.Attack} damage.\nPassive: When this unit survives combat, +1 gold.");
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
        RunManager.Instance.currentGold+=1;
        Debug.Log("Farmer just farmed 1 gold!");
    }


}
