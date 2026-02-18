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
    }
}