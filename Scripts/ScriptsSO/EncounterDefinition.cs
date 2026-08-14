using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Encounter", menuName = "Game/Encounter Definition")]
public class EncounterDefinition : ScriptableObject
{
    public string encounterName;
    public List<RunManager.UnitPlacement> enemyUnits = new();
    public List<RunManager.TacticPlacement> enemyTactics = new();
}

