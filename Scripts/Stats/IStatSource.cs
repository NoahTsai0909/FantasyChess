public interface IStatSource
{
    int Attack { get; }
    int Heal { get; }
    int MaxHP { get; }
    float Cooldown { get; }

    int Shield { get; }

    int Burn { get; }

    int Poison { get; }

    int MaxEnergy { get; }

    int Slow { get; }

    int Haste { get; }  

    int Multicast { get; }

    int Value { get; }
}

