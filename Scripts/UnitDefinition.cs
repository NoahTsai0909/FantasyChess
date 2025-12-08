using UnityEngine;

[CreateAssetMenu(fileName = "UnitDefinition", menuName = "Units/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Basic Stats")]
    public string unitName;
    public int maxHP;

    [Header("Combat Values")]
    public int attack;
    public int healValue;

    [Header("Cooldown")]
    public float Cooldown;
    public bool isPassive;

    [Header("Visuals")]
    public Sprite unitSprite;

    [Header("Meta")]
    public int cost;
    public string region;   // Aurelia / Nethervale / Everborn / Axiom
}
