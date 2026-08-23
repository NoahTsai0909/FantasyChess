using System;
using UnityEngine;

[Serializable]
public class UnitCombatStats
{
    public Guid UnitId;
    public string UnitName;
    public bool IsPlayer;
    public Sprite UnitIcon;

    // --- DAMAGE TAB ---
    public int DirectDamageDealt;
    public int BurnDamageDealt;
    public int PoisonDamageDealt;

    public int TotalDamageDealt => DirectDamageDealt + BurnDamageDealt + PoisonDamageDealt;

    // --- MITIGATION TAB ---
    public int DamageTaken;
    public int HealingDone;
    public int ShieldingDone;

    // --- UTILITY TAB ---
    public int SlowsApplied;
    public int HastesApplied;
    public int AdvancesGiven;

    public UnitCombatStats(Guid id, string name, bool isPlayer, Sprite icon)
    {
        UnitId = id;
        UnitName = name;
        IsPlayer = isPlayer;
        UnitIcon = icon;
    }
}
