using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RegionLevelTree", menuName = "Game/Progression/Region Level Tree")]
public class RegionLevelTreeSO : ScriptableObject
{
    [Tooltip("The name of the region (e.g., 'Solmire')")]
    public Region regionName;

    public List<LevelUpEventSO> levelNodes = new List<LevelUpEventSO>();
}