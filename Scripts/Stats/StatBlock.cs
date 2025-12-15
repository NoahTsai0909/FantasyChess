using UnityEngine;
[System.Serializable]
public class StatBlock
{
    private UnitDefinition definition;
    private PermanentStats permanent;
    private TemporaryStats temporary;

    public StatBlock(UnitDefinition def, PermanentStats perm, TemporaryStats temp)
    {
        definition = def;
        permanent = perm;
        temporary = temp;
    }

    public int Attack =>
        definition.attack
        + permanent.bonusAttack
        + temporary.attackBonus;

    public int Heal =>
        definition.healValue
        + permanent.bonusHeal
        + temporary.healBonus;

    public int MaxHP =>
        definition.maxHP
        + permanent.bonusMaxHP
        + temporary.maxHPBonus;

    public float Cooldown =>
        Mathf.Max(0.5f,
            definition.Cooldown
            - permanent.cooldownReduction
            - temporary.cooldownDelta
        );
}

