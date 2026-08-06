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
        RunHUDManager.Instance?.Hide();
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
        List<System.Guid> survivingGuids = new List<System.Guid>();
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (placement.unitData != null) survivingGuids.Add(placement.unitData.id);
        }

        var survivingStats = new List<UnitLifetimeStats>();
        foreach (var guid in survivingGuids)
        {
            if (RunManager.Instance.masterUnitStats.ContainsKey(guid))
            {
                survivingStats.Add(RunManager.Instance.masterUnitStats[guid]);
            }
        }

        var sortedStats = survivingStats.OrderByDescending(stat => stat.ContributionScore).ToList();

        for (int i = 0; i < sortedStats.Count; i++)
        {
            var statData = sortedStats[i];

            Sprite unitSprite = null;
            var placement = RunManager.Instance.playerTeamPlacements.FirstOrDefault(p => p.unitData != null && p.unitData.id == statData.id);

            if (placement != null && placement.unitData.definition != null)
            {
                unitSprite = placement.unitData.definition.unitSprite;
            }

            // Instantiate and Setup
            GameObject statBlock = Instantiate(unitStatBlockPrefab, unitStatsContentParent);
            UnitStatBlockUI uiComponent = statBlock.GetComponent<UnitStatBlockUI>();

            if (uiComponent != null)
            {
                // If i == 0, they are the MVP!
                uiComponent.Setup(statData, unitSprite, isMVP: i == 0);
            }
        }
    }
}
