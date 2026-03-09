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
    public int bonusMaxEnergy;
    public int bonusSlow;
    public int bonusHaste;
    public int bonusMulticast;
    public int bonusValue;

    public void Reset()
    {
        bonusAttack = 0;
        bonusHeal = 0;
        bonusMaxHP = 0;
        cooldownReduction = 0f;
        bonusShield = 0;
        bonusBurn = 0;
        bonusPoison = 0;
        bonusMaxEnergy = 0;
        bonusSlow = 0;
        bonusHaste = 0;
        bonusMulticast = 0;
        bonusValue = 0;
    }
}
