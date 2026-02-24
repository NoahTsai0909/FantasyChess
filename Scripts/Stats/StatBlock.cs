using UnityEngine;
[System.Serializable]

public enum ModifiableStats
{
    Attack,
    Heal,
    MaxHP,
    Cooldown,
    Shield,
    Burn,
    Poison,
    Slow,
    Haste,
    MaxEnergy,
    Multicast
}

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

    private int PermMaxEnergy => permanent?.bonusMaxEnergy ?? 0;

    private int PermSlow => permanent?.bonusSlow ?? 0;  

    private int PermHaste => permanent?.bonusHaste ?? 0;

    private int PermMulticast => permanent?.bonusMulticast ?? 0;

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

    public int maxEnergy => baseStats.MaxEnergy + PermMaxEnergy + temporary.maxEnergyBonus;

    public int Slow => baseStats.Slow + PermSlow + temporary.slowBonus;

    public int Haste => baseStats.Haste + PermHaste + temporary.hasteBonus;

    public int Multicast => baseStats.Multicast + PermMulticast + temporary.multicastBonus;
    public float Cooldown =>
        Mathf.Max(
            0.5f,
            baseStats.Cooldown
            - PermCooldownReduction
            - temporary.cooldownDelta
        );
}


