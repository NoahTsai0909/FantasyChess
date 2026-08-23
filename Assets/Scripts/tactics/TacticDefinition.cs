using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TacticDefinition", menuName = "Tactics/Tactic Definition")]
public class TacticDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string tacticName;

    [Header("Behavior")]
    public bool isPassive;
    [Tooltip("If active, how many seconds before this tactic executes?")]
    public float cooldown;

    [Header("Visuals")]
    public Sprite tacticSprite;

    [Header("Meta")]
    public List<Region> regions = new List<Region>();
    public Rarity rarity;
    public Rarity startingRarity;

    [Header("Exclusivity")]
    [Tooltip("If true, this tactic normally only drops from combat rewards and is excluded from standard shops.")]
    public bool isCombatExclusive = false;
    public bool isEventExclusive = false;

    [Header("Prefab Reference")]
    public TacticInstance tacticPrefab;
}
