using UnityEngine;

[CreateAssetMenu(fileName = "UnitDefinition", menuName = "Units/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Basic Stats")]
    public string unitName;
    public int maxHP;
    public int attack;
    public float Cooldown;

    [Header("Visuals")]
    public Sprite unitSprite;

    [Header("Meta")]
    public int cost;
    public string region;   // Aurelia / Nethervale / Everborn / Axiom
}
