[System.Flags]
public enum UnitTagFlags
{
    None = 0,
    Tank = 1 << 0,
    Damage = 1 << 1,
    Healer = 1 << 2,
    Support = 1 << 3,
    Melee = 1 << 4,
    Ranged = 1 << 5,
    Magic = 1 << 6,
    Warrior = 1 << 7,
    Weaponry = 1 << 8,
}
