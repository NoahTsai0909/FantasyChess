[System.Serializable]
public class PermanentStats
{
    public int bonusAttack;
    public int bonusHeal;
    public int bonusMaxHP;
    public float cooldownReduction;
    public int bonusShield;
    public int bonusBurn;
    public int bonusPoison;

    public void Reset()
    {
        bonusAttack = 0;
        bonusHeal = 0;
        bonusMaxHP = 0;
        cooldownReduction = 0f;
        bonusShield = 0;
        bonusBurn = 0;
        bonusPoison = 0;
    }
}
