[System.Serializable]
public class TemporaryStats
{
    public int attackBonus;
    public int healBonus;
    public int maxHPBonus;
    public float cooldownDelta;
    public int shieldBonus;
    public int burnBonus;
    public int poisonBonus;
    public int maxEnergyBonus;
    public int slowBonus;
    public int hasteBonus;
    public int multicastBonus;
    public int valueBonus;
    public int critChanceBonus;

    public void Clear()
    {
        attackBonus = 0;
        healBonus = 0;
        maxHPBonus = 0;
        cooldownDelta = 0f;
        shieldBonus = 0;
        burnBonus = 0;
        poisonBonus = 0;
        maxEnergyBonus = 0;
        slowBonus = 0;
        hasteBonus = 0;
        multicastBonus = 0;
        valueBonus = 0;
        critChanceBonus = 0;
    }
}