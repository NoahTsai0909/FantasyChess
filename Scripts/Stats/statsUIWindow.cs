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
    [SerializeField] private ScrollRect scrollRect;

    [Header("Tab Buttons")]
    [SerializeField] private Button damageButton;
    [SerializeField] private Button mitigationButton;
    [SerializeField] private Button utilityButton;
    [SerializeField] private Button returnButton;

    [Header("Colors - Damage")]
    public Color directDmgColor = new Color(0.8f, 0.2f, 0.2f); // Red
    public Color burnDmgColor = new Color(1f, 0.5f, 0f);       // Orange
    public Color poisonDmgColor = new Color(0.2f, 0.8f, 0.2f); // Green

    [Header("Colors - Mitigation")]
    public Color dmgTakenColor = new Color(0.5f, 0.5f, 0.5f);  // Gray
    public Color healColor = new Color(0.2f, 0.8f, 0.2f);      // Green
    public Color shieldColor = new Color(0.2f, 0.6f, 1f);      // Blue

    [Header("Colors - Utility")]
    public Color slowColor = new Color(0.6f, 0.8f, 1f);        // Ice Blue
    public Color hasteColor = new Color(1f, 0.8f, 0.2f);       // Yellow
    public Color advanceColor = new Color(0.8f, 0.2f, 0.8f);

    private StatTab currentTab = StatTab.Damage;
    private Dictionary<Guid, StatRowUI> activeRows = new Dictionary<Guid, StatRowUI>();

    private void Awake()
    {
        damageButton.onClick.AddListener(() => { currentTab = StatTab.Damage; RefreshUI(); ResetScroll(); });
        mitigationButton.onClick.AddListener(() => { currentTab = StatTab.Mitigation; RefreshUI(); ResetScroll(); });
        utilityButton.onClick.AddListener(() => { currentTab = StatTab.Utility; RefreshUI(); ResetScroll(); });
        returnButton.onClick.AddListener(() => { this.gameObject.SetActive(false); });
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

        allStats.Sort((a, b) => GetPrimaryStatForTab(b).CompareTo(GetPrimaryStatForTab(a)));

        int maxValue = 0;
        foreach (var stat in allStats)
        {
            int val = GetPrimaryStatForTab(stat);
            if (val > maxValue) maxValue = val;
        }

        for (int i = 0; i < allStats.Count; i++)
        {
            var stat = allStats[i];

            if (!activeRows.ContainsKey(stat.UnitId))
            {
                StatRowUI newRow = Instantiate(rowPrefab, rowContainer);
                activeRows[stat.UnitId] = newRow;
            }

            StatRowUI row = activeRows[stat.UnitId];
            row.transform.SetSiblingIndex(i);

            // Package the segmented data based on the current tab
            List<(int, Color)> segments = new List<(int, Color)>();
            if (currentTab == StatTab.Damage)
            {
                segments.Add((stat.DirectDamageDealt, directDmgColor));
                segments.Add((stat.BurnDamageDealt, burnDmgColor));
                segments.Add((stat.PoisonDamageDealt, poisonDmgColor));
            }
            else if (currentTab == StatTab.Mitigation)
            {
                segments.Add((stat.DamageTaken, dmgTakenColor));
                segments.Add((stat.HealingDone, healColor));
                segments.Add((stat.ShieldingDone, shieldColor));
            }
            else if (currentTab == StatTab.Utility)
            {
                segments.Add((stat.SlowsApplied, slowColor));
                segments.Add((stat.HastesApplied, hasteColor));
                segments.Add((stat.AdvancesGiven, advanceColor));
            }

            row.UpdateRow(stat.UnitIcon, GetPrimaryStatForTab(stat), maxValue, segments);
        }
    }

    private int GetPrimaryStatForTab(UnitCombatStats stats)
    {
        switch (currentTab)
        {
            case StatTab.Damage:
                return stats.TotalDamageDealt;
            case StatTab.Mitigation:
                // Summing these ensures the bar reflects TOTAL mitigation activity
                return stats.DamageTaken + stats.HealingDone + stats.ShieldingDone;
            case StatTab.Utility:
                return stats.SlowsApplied + stats.HastesApplied + stats.AdvancesGiven;
            default:
                return 0;
        }
    }

    private void ResetScroll()
    {
        // FORCE UNITY TO CALCULATE SIZES BEFORE SCROLLING!
        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
