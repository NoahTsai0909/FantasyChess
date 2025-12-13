using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Team", menuName = "Game/Team Definition")]
public class TeamDefinition : ScriptableObject
{
    public string teamName;
    public List<UnitPlacement> units = new List<UnitPlacement>();

    [System.Serializable]
    public class UnitPlacement
    {
        public UnitInstance unitPrefab; // CHANGE: Store PREFAB, not definition
        public int row;
        public int col;
    }
}
