using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatsUIWindow : MonoBehaviour
{
    public enum StatTab { Damage, Mitigation, Utility }

    [Header("UI References")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private StatRowUI rowPrefab;

    [Header("Tab Buttons")]
    [SerializeField] private Button damageButton;
    [SerializeField] private Button mitigationButton;
    [SerializeField] private Button utilityButton;

    private StatTab currentTab = StatTab.Damage;
    private Dictionary<Guid, StatRowUI> activeRows = new Dictionary<Guid, StatRowUI>();

    private void Awake()
    {
        damageButton.onClick.AddListener(() => { currentTab = StatTab.Damage; RefreshUI(); });
        mitigationButton.onClick.AddListener(() => { currentTab = StatTab.Mitigation; RefreshUI(); });
        utilityButton.onClick.AddListener(() => { currentTab = StatTab.Utility; RefreshUI(); });
    }
    private void Start()
    {
        // Start runs after all instances are definitely initialized!
        if (CombatStatsTracker.Instance != null)
        {
            CombatStatsTracker.Instance.OnStatsUpdated += RefreshUI;
        }
    }
    private void OnDestroy()
    {
        if (CombatStatsTracker.Instance != null)
        {
            CombatStatsTracker.Instance.OnStatsUpdated -= RefreshUI;
        }
    }


    private void RefreshUI()
    {
        if (CombatStatsTracker.Instance == null) return;
        var allStats = CombatStatsTracker.Instance.GetAllStats().Values.ToList();
        if (allStats.Count == 0) return;

        // 1. Sort the list based on the currently selected tab
        allStats.Sort((a, b) => GetPrimaryStatForTab(b).CompareTo(GetPrimaryStatForTab(a)));

        // 2. Find the highest value to scale the fill bars correctly
        int maxValue = 0;
        foreach (var stat in allStats)
        {
            int val = GetPrimaryStatForTab(stat);
            if (val > maxValue) maxValue = val;
        }

        // 3. Update or Instantiate rows
        for (int i = 0; i < allStats.Count; i++)
        {
            var stat = allStats[i];

            // If we haven't created a row for this unit yet, make one
            if (!activeRows.ContainsKey(stat.UnitId))
            {
                StatRowUI newRow = Instantiate(rowPrefab, rowContainer);
                activeRows[stat.UnitId] = newRow;
            }

            StatRowUI row = activeRows[stat.UnitId];

            // Force the layout to respect the sorted order
            row.transform.SetSiblingIndex(i);

            // Update the text and the bar
            row.UpdateRow(stat.UnitIcon, GetPrimaryStatForTab(stat), maxValue, stat.IsPlayer);
        }
    }

    private int GetPrimaryStatForTab(UnitCombatStats stats)
    {
        switch (currentTab)
        {
            case StatTab.Damage:
                return stats.TotalDamageDealt;

            case StatTab.Mitigation:
                // You could change this to display Healing/Shielding instead based on sub-tabs
                return stats.DamageTaken;

            case StatTab.Utility:
                // Combining them for a general "Utility Score" or you can isolate one
                return stats.SlowsApplied + stats.HastesApplied + stats.AdvancesGiven;

            default:
                return 0;
        }
    }
}
