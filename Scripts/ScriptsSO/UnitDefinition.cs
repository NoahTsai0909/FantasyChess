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
    public int poison;
    public int slow;
    public int haste;

    [Header("Cooldown")]
    public float cooldown;
    public bool isPassive;
    public bool isEnergy;
    public int maxEnergy;
    public int multicast = 1;

    [Header("Visuals")]
    public Sprite unitSprite;
    public List<Sprite> defaultProjectile;

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

    public int Shield => shield;
    public int Burn => burn;
    public int Poison => poison;

    public int MaxEnergy => maxEnergy;

    public int Slow => slow;
    public int Haste => haste;

    public int Multicast => multicast;

    public int Value => 0;
}

public enum Rarity { Common, Uncommon, Rare, Epic }
public enum Region { Solmire, Nethervale, Everborn, Axiom, None}