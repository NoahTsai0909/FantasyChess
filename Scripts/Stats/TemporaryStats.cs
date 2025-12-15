[System.Serializable]
public class TemporaryStats
{
    public int attackBonus;
    public int healBonus;
    public int maxHPBonus;
    public float cooldownDelta;

    public void Clear()
    {
        attackBonus = 0;
        healBonus = 0;
        maxHPBonus = 0;
        cooldownDelta = 0f;
    }
}