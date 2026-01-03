using UnityEngine;
[System.Serializable]
public class StatBlock
{
    private IStatSource baseStats;
    private PermanentStats permanent;
    private TemporaryStats temporary;

    public StatBlock(IStatSource baseStats, PermanentStats perm, TemporaryStats temp)
    {
        this.baseStats = baseStats;
        permanent = perm ?? new PermanentStats();
        temporary = temp ?? new TemporaryStats();
    }

    private int PermAttack => permanent?.bonusAttack ?? 0;
    private int PermHeal => permanent?.bonusHeal ?? 0;
    private int PermMaxHP => permanent?.bonusMaxHP ?? 0;
    private float PermCooldownReduction => permanent?.cooldownReduction ?? 0f;

    public int Attack =>
        baseStats.Attack
        + PermAttack
        + temporary.attackBonus;

    public int Heal =>
        baseStats.Heal
        + PermHeal
        + temporary.healBonus;

    public int MaxHP =>
        baseStats.MaxHP
        + PermMaxHP
        + temporary.maxHPBonus;

    public float Cooldown =>
        Mathf.Max(
            0.5f,
            baseStats.Cooldown
            - PermCooldownReduction
            - temporary.cooldownDelta
        );
}


