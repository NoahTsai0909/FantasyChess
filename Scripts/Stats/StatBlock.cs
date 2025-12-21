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
        permanent = perm;
        temporary = temp;
    }

    public int Attack =>
        baseStats.Attack + permanent.bonusAttack + temporary.attackBonus;

    public int Heal =>
        baseStats.Heal + permanent.bonusHeal + temporary.healBonus;

    public int MaxHP =>
        baseStats.MaxHP + permanent.bonusMaxHP + temporary.maxHPBonus;

    public float Cooldown =>
        Mathf.Max(0.5f,
            baseStats.Cooldown
            - permanent.cooldownReduction
            - temporary.cooldownDelta
        );
}


