public static class RarityScaling
{
    // delta = CurrentRarity - StartingRarity
    public static float GetMultiplier(int rarityDelta)
    {
        return rarityDelta switch
        {
            0 => 1.0f,   // starting rarity
            1 => 1.5f,   // +1 tier
            2 => 2.25f,  // +2 tiers
            3 => 3.5f,   // +3 tiers
            _ => 1.0f
        };
    }

    public static Rarity GetNextRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Rarity.Uncommon;
            case Rarity.Rare:
                return Rarity.Rare;
            case Rarity.Epic:
                return Rarity.Epic;
            default:
                return Rarity.Epic;
        }
    }

}

