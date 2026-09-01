using UnityEngine;

[CreateAssetMenu(fileName = "New Adventure", menuName = "Adventure/Adventure Definition")]
public class AdventureDefinitionSO : ScriptableObject
{
    [Header("Basic Info")]
    public string adventureName;
    [TextArea] public string description;
    public Sprite artworkCard;
    [Header("Run Rules")]
    public int totalDays = 12;
    public int startingGold = 10;
    public int startingHealth = 12;
    public int startingProvisionCap = 4;
}