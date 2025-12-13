using System.Collections.Generic;
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
    public Rarity rarity;
    public List<string> tags; // ["weaponry", "ranged", "support", "tank"]
    public int reputationUnlockLevel;

    [Header("Prefab Reference")]
    public UnitInstance unitPrefab;

    [Header("Tags")]
    public UnitTagFlags tagFlags;
}

public enum Rarity { Common, Rare, Epic, Legendary }