using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneLoader;

public class RunSummaryController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI generalStatsText;
    [SerializeField] private Transform unitStatsContentParent; // The Scroll View Content
    [SerializeField] private GameObject unitStatBlockPrefab; // A UI prefab you design for individual unit stats
    [SerializeField] private Button mainMenuButton;

    [Header("Grid Reference")]
    [SerializeField] private GridManager summaryGrid;

    void Start()
    {
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                if (RunManager.Instance != null)
                {
                    RunManager.Instance.ResetRun();
                }
                SceneLoader.Instance.LoadScene(GameScene.MainMenuScene);
            });
        }

        if (RunManager.Instance == null) return;

        DisplayGeneralStats();
        SpawnSurvivingTeam();
        DisplaySortedUnitStats();
    }

    private void DisplayGeneralStats()
    {
        // Pull from RunManager.Stats
        var stats = RunManager.Instance.Stats;

        generalStatsText.text = $"<b>Run Summary</b>\n" +
                                $"Days Survived: {stats.CurrentDay}\n" +
                                $"Final Gold: {stats.CurrentGold}\n" +
                                $"Events Completed: {RunManager.Instance.regularEventsCompleted}\n";
    }

    private void SpawnSurvivingTeam()
    {
        // 1. Loop through the final team saved in the RunManager
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (placement.unitData == null || placement.unitData.definition == null) continue;

            // 2. Spawn the visual unit
            UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);
            unit.InitializeFromSaveData(placement.unitData);

            // 3. Place them on the summary grid passively (no combat logic)
            summaryGrid.PlaceUnit(placement, placement.row, placement.col, unit, true);
        }
    }

    private void DisplaySortedUnitStats()
    {
        // 1. Get all the GUIDs of the surviving units
        List<System.Guid> survivingGuids = new List<System.Guid>();
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (placement.unitData != null)
            {
                survivingGuids.Add(placement.unitData.id); // Assuming UnitSaveData has the GUID
            }
        }

        // 2. Filter the master stats dictionary to only include survivors
        var survivingStats = new List<UnitLifetimeStats>();
        foreach (var guid in survivingGuids)
        {
            if (RunManager.Instance.masterUnitStats.ContainsKey(guid))
            {
                survivingStats.Add(RunManager.Instance.masterUnitStats[guid]);
            }
        }

        // 3. Sort them descending by your new ContributionScore!
        var sortedStats = survivingStats.OrderByDescending(stat => stat.ContributionScore).ToList();

        // 4. Spawn a UI block for each one in the scroll view
        for (int i = 0; i < sortedStats.Count; i++)
        {
            var statData = sortedStats[i];

            // Instantiate your UI prefab into the scroll view
            GameObject statBlock = Instantiate(unitStatBlockPrefab, unitStatsContentParent);

            // E.g., statBlock.GetComponent<UnitStatUI>().Setup(statData, rank: i + 1);
        }
    }
}
