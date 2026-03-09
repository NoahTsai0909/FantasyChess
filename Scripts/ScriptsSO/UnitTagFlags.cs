[System.Flags]
public enum UnitTagFlags
{
    None = 0,
    Beast = 1 << 0,
    Construct = 1 << 1,
    Mage = 1 << 2,
    Heal =  1 << 3,
    Haste = 1 << 4,
    Burn = 1 << 5,
    Poison = 1 << 6,
    Slow = 1 << 7,
    Shield = 1 << 8,
    Energy = 1 << 9,
    Consumable = 1 << 10,
}
