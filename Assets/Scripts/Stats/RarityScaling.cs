public static class RarityScaling
{
    // delta = CurrentRarity - StartingRarity
    public static float GetMultiplier(int rarityDelta)
    {
        return rarityDelta switch
        {
            0 => 1.0f,   // starting rarity
            1 => 1.5f,   // +1 tier
            2 => 2.5f,  // +2 tiers
            3 => 4.5f,   // +3 tiers
            _ => 1.0f
        };
    }

    public static float GetMaxHPMultiplier(int rarityDelta)
    {
        return rarityDelta switch
        {
            0 => 1.0f,   // starting rarity
            1 => 1.7f,   // +1 tier
            2 => 2.9f,  // +2 tiers
            3 => 4.9f, // +3 tiers
            _ => 1.0f
        };
    }

    public static Rarity GetNextRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Rarity.Uncommon;
            case Rarity.Uncommon:
                return Rarity.Rare;
            case Rarity.Rare:
                return Rarity.Epic;
            default:
                return Rarity.Epic;
        }
    }

}

