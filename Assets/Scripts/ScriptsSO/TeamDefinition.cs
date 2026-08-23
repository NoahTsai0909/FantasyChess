using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Team", menuName = "Game/Team Definition")]
public class TeamDefinition : ScriptableObject
{
    public string teamName;
    public List<RunManager.UnitPlacement> units = new();
}
