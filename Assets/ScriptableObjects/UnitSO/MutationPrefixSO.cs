using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Mutations/Prefix")]
public class MutationPrefixSO : ScriptableObject
{
    public string prefixName; // e.g., "Flaming"
    public ModifiableStats statToGrant; // e.g., ModifiableStats.Burn
    public float conversionWeight;
    public int flatBonusAmount = 5;

    public List<MutationSuffixSO> allowedSuffixes = new List<MutationSuffixSO>();

    public Sprite runeSprite; 
    public Color runeColor = Color.white;
    public GameObject particlePrefab;
}
