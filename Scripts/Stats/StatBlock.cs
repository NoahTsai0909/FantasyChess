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

    private int PermShield => permanent?.bonusShield ?? 0; 

    private int PermBurn => permanent?.bonusBurn ?? 0;

    private int PermPoison => permanent?.bonusPoison ?? 0;

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

    public int Shield =>
        baseStats.Shield + PermShield + temporary.shieldBonus;

    public int Burn => baseStats.Burn + PermBurn + temporary.burnBonus;

    public int Poison => baseStats.Poison  + PermPoison + temporary.poisonBonus;
    public float Cooldown =>
        Mathf.Max(
            0.5f,
            baseStats.Cooldown
            - PermCooldownReduction
            - temporary.cooldownDelta
        );
}


