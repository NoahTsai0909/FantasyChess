using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    // Run Data
    public int currentGold = 100;
    public int currentNodeIndex = 0;

    public List<UnitPlacement> playerTeamPlacements = new List<UnitPlacement>();

    [System.Serializable]
    public class UnitPlacement
    {
        public UnitInstance unitPrefab;
        public int row;
        public int col;
    }

    [Header("UI Manager")]
    [SerializeField] private UnitInstance defaultUnitPrefab;
    [SerializeField] private int defaultUnitRow;
    [SerializeField] private int defaultUnitCol;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (playerTeamPlacements.Count == 0)
            {
                InitializeDefaultTeam();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDefaultTeam()
    {
        playerTeamPlacements.Add(new UnitPlacement
        {
            unitPrefab = defaultUnitPrefab,
            row = defaultUnitRow,
            col = defaultUnitCol
        });
    }

    public TeamDefinition GetTeamForCombat()
    {
        // Create a temporary TeamDefinition ScriptableObject
        TeamDefinition combatTeam = ScriptableObject.CreateInstance<TeamDefinition>();
        combatTeam.teamName = "Player Team";

        // Copy placements
        foreach (var placement in playerTeamPlacements)
        {
            combatTeam.units.Add(new TeamDefinition.UnitPlacement
            {
                unitPrefab = placement.unitPrefab,
                row = placement.row,
                col = placement.col
            });
        }

        return combatTeam;
    }
}
