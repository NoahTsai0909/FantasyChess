public class UnitDefinitionView : IStatSource
{
    public int Attack { get; private set; }
    public int Heal { get; private set; }
    public int MaxHP { get; private set; }
    public float Cooldown { get; private set; }

    public int Shield { get; private set; }

    public int Burn {  get; private set; }

    public int Poison { get; private set; }

    public UnitDefinitionView(int attack, int heal, int maxHP, float cooldown, int shield, int burn, int poison)
    {
        Attack = attack;
        Heal = heal;
        MaxHP = maxHP;
        Cooldown = cooldown;
        Shield = shield;
        Burn = burn;
        Poison = poison;
    }
}

