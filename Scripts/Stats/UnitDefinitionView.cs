public class UnitDefinitionView : IStatSource
{
    public int Attack { get; private set; }
    public int Heal { get; private set; }
    public int MaxHP { get; private set; }
    public float Cooldown { get; private set; }

    public int Shield { get; private set; }

    public int Burn {  get; private set; }

    public int Poison { get; private set; }

    public int MaxEnergy { get; private set; }

    public int Slow { get; private set; }

    public int Haste { get; private set; }

    public int Multicast { get; private set; }

    public int Value { get; private set; }

    public int CritChance { get; private set; }

    public UnitDefinitionView(int attack, int heal, int maxHP, float cooldown, int shield, int burn, int poison, int maxEnergy, int slow, int haste, int multicast, int value, int critChance)
    {
        Attack = attack;
        Heal = heal;
        MaxHP = maxHP;
        Cooldown = cooldown;
        Shield = shield;
        Burn = burn;
        Poison = poison;
        MaxEnergy = maxEnergy;
        Slow = slow;
        Haste = haste;
        Multicast = multicast;
        Value = value;
        CritChance = critChance;
    }
}

