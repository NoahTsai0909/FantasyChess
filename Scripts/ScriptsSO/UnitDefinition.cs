using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDefinition", menuName = "Units/Unit Definition")]
public class UnitDefinition : ScriptableObject, IStatSource
{
    [Header("Basic Stats")]
    public string unitName;
    public int maxHP;

    [Header("Combat Values")]
    public int attack;
    public int heal;
    public int shield;
    public int burn;

    [Header("Cooldown")]
    public float cooldown;
    public bool isPassive;

    [Header("Visuals")]
    public Sprite unitSprite;

    [Header("Meta")]
    public int cost;
    public Region region;   // Aurelia / Nethervale / Everborn / Axiom
    public Rarity rarity;
    public Rarity startingRarity;
    public int provisionCost;

    [Header("Prefab Reference")]
    public UnitInstance unitPrefab;

    [Header("Tags")]
    public UnitTagFlags tagFlags;

    public int Attack => attack;
    public int Heal => heal;
    public int MaxHP => maxHP;

    public float Cooldown => cooldown;
}

public enum Rarity { Common, Uncommon, Rare, Epic }
public enum Region { Solmire, Nethervale, Everborn, Axiom}