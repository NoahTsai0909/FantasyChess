using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Encounter", menuName = "Game/Encounter Definition")]
public class EncounterDefinition : ScriptableObject
{
    public string encounterName;
    public List<UnitPlacement> enemyUnits = new List<UnitPlacement>();

    [System.Serializable]
    public class UnitPlacement
    {
        public UnitInstance unitPrefab; // CHANGE: Store PREFAB, not definition
        public int row;
        public int col;
    }
}

