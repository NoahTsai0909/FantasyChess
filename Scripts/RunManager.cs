using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static SceneLoader;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    // Run Data
    public RunStats Stats = new RunStats();

    public List<UnitPlacement> playerTeamPlacements = new();
    public List<UnitPlacement> playerBenchPlacements = new();
    public List<TacticPlacement> playerTactics = new();

    [System.Serializable]
    public class UnitPlacement
    {
        public UnitSaveData unitData;
        public int row;
        public int col;
    }

    [System.Serializable]
    public class TacticSaveData
    {
        public TacticDefinition definition;
        public Rarity rarity;
        public Guid id = Guid.NewGuid();
    }

    [System.Serializable]
    public class TacticPlacement
    {
        public TacticSaveData tacticData;
        public int orderIndex; // Their place in the timeline (0 to N)
    }
    public class ShopState
    {
        public List<UnitSaveData> offeredUnits;
        public HashSet<UnitDefinition> purchasedUnits = new();
        public int currentPage;
        public bool hasRefreshed;
        public int minProvisionFilter;  
        public int maxProvisionFilter;
    }


    [Header("Default Unit")]
    [SerializeField] private List<UnitPlacement> defaultUnits;
    [SerializeField] private int benchSize = 9;

    public int regularEventsCompleted = 0;
    public const int REGULAR_EVENTS_PER_DAY = 3;
    public bool isBattlePhase = false;
    public int currentEventPhase = 0;
    

    // New: Day event tracking
    public List<BaseEventSO> currentDailyEvents = new();  // Current 3 events to display
    public List<BaseEventSO> allDayEvents = new();

    public BaseEventSO selectedEvent;
    public EncounterDefinition currentEncounter;
    public bool eventInProgress = false;
    public const int TOTAL_DAYS = 12;
    public ShopState shopState;


    private Dictionary<Guid, PermanentStats> permanentStatsMap = new();
    public Dictionary<Guid, UnitLifetimeStats> masterUnitStats = new Dictionary<Guid, UnitLifetimeStats>();
    [SerializeField] public RarityDistributionTable rarityDistributionTable;
    [Header("Region Progression")]
    public Region playerRegion;
    public RegionLevelTreeSO currentRegionTree;
    [Tooltip("Drag all your specific RegionLevelTreeSO assets into this list!")]
    [SerializeField] private List<RegionLevelTreeSO> allRegionTrees = new List<RegionLevelTreeSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AssignRegionTree();

            if (playerTeamPlacements.Count == 0)
                InitializeDefaultTeam();

            InitializeBench();
            SanitizeBench();

            Stats.Initialize();
            regularEventsCompleted = 0;
            isBattlePhase = false;
            currentEventPhase = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void AssignRegionTree()
    {
        currentRegionTree = null; // Clear any old data

        foreach (var tree in allRegionTrees)
        {
            if (tree.regionName == playerRegion)
            {
                currentRegionTree = tree;
                Debug.Log($"Successfully loaded Level Tree for region: {playerRegion}");
                return;
            }
        }

        Debug.LogWarning($"Could not find a RegionLevelTreeSO for the region: {playerRegion}!");
    }

    void InitializeDefaultTeam()
    {
        playerTeamPlacements.Clear();

        foreach (var placement in defaultUnits)
        {
            if (placement == null || placement.unitData == null || placement.unitData.definition == null)
            {
                Debug.LogError("Default unit placement is invalid!");
                continue;
            }

            // Clone placement so runtime changes don't mutate the asset
            playerTeamPlacements.Add(new UnitPlacement
            {
                unitData = new UnitSaveData
                {
                    definition = placement.unitData.definition,
                    rarity = placement.unitData.rarity,
                    // GUID is generated automatically in constructor / factory
                },
                row = placement.row,
                col = placement.col
            });
        }
    }


    void InitializeBench()
    {
        playerBenchPlacements.Clear();

        if (playerBenchPlacements == null || playerBenchPlacements.Count != benchSize)
        {
            playerBenchPlacements = new List<UnitPlacement>(benchSize);
            for (int i = 0; i < benchSize; i++)
            {
                playerBenchPlacements.Add(new UnitPlacement());
            }
        }
    }

    public void SanitizeBench()
    {
        for (int i = 0; i < playerBenchPlacements.Count; i++)
        {
            var placement = playerBenchPlacements[i];

            if (placement.unitData != null &&
                placement.unitData.definition == null)
            {
                placement.unitData = null;
            }
        }
    }


    public TeamDefinition GetTeamForCombat()
    {
        TeamDefinition team = ScriptableObject.CreateInstance<TeamDefinition>();
        team.teamName = "Player Team";

        foreach (var placement in playerTeamPlacements)
        {
            if (placement.unitData == null) continue;

            team.units.Add(new UnitPlacement
            {
                unitData = placement.unitData,
                row = placement.row,
                col = placement.col
            });
        }

        return team;
    }

    public TeamDefinition GetTeamForBench()
    {
        TeamDefinition benchTeam = ScriptableObject.CreateInstance<TeamDefinition>();
        benchTeam.teamName = "Player Bench";

        if (benchTeam.units == null)
            benchTeam.units = new List<UnitPlacement>();

        foreach (var placement in playerBenchPlacements)
        {
            benchTeam.units.Add(new UnitPlacement
            {
                unitData = placement.unitData,
                row = -1,
                col = -1
            });
        }

        return benchTeam;
    }

    public void StartNewDay()
    {
        Stats.CurrentDay++;
        regularEventsCompleted = 0;
        isBattlePhase = false;
        currentEventPhase = 0;
        allDayEvents.Clear();
        currentDailyEvents.Clear();

        GenerateDailyEvents();
    }

    public void CompleteRegularEvent()
    {
        regularEventsCompleted++;
        Stats.Experience++;

        // Move to next regular event phase
        currentEventPhase++;

        // Check if we've completed all 3 regular events for this day
        if (regularEventsCompleted >= REGULAR_EVENTS_PER_DAY)
        {
            // Time for the battle phase!
            isBattlePhase = true;
            Debug.Log("All 3 regular events completed! Moving to battle phase!");
        }
        else
        {
            // Still in regular events, show the next set of 3 choices
            isBattlePhase = false;
        }

        // Generate next set of events (will be either next 3 regular events, or combat events)
        GenerateDailyEvents();
    }

    public void CompleteBattleEvent()
    {
        // Battle completed! This ends the current day
        isBattlePhase = false;
        regularEventsCompleted = 0;
        currentEventPhase = 0;
        allDayEvents.Clear();

        if (Stats.CurrentDay >= TOTAL_DAYS)
        {
            SceneLoader.Instance.LoadScene(GameScene.RunSummaryScene);
            return;
        }
        StartNewDay();
    }


    public void GenerateDailyEvents()
    {
        Debug.Log($"=== GenerateDailyEvents START ===");
        Debug.Log($"Day {Stats.CurrentDay}, Phase: {(isBattlePhase ? "BATTLE" : $"REGULAR {currentEventPhase + 1}/3")}, Regular events completed: {regularEventsCompleted}");

        EventPoolManager eventPool = EventPoolManager.Instance;

        if (eventPool == null)
        {
            Debug.LogError("EventPoolManager.Instance is NULL!");
            return;
        }

        if (isBattlePhase)
        {
            // Battle phase: generate 3 combat events for the player to choose from
            Debug.Log("GENERATING COMBAT EVENTS (Battle Phase)");
            currentDailyEvents = eventPool.GetCombatEvents(3);
            allDayEvents.Clear();  // Battle phase doesn't use the regular event pool
            Debug.Log($"Got {currentDailyEvents.Count} combat events for battle");

            foreach (var ev in currentDailyEvents)
            {
                Debug.Log($"  Battle option: {ev.eventName}");
            }
        }
        else
        {
            // Regular event phase: need to show 3 choices
            // Generate ALL 9 regular events for the day if we haven't already
            if (allDayEvents.Count == 0)
            {
                Debug.Log($"GENERATING ALL 9 REGULAR EVENTS FOR DAY {Stats.CurrentDay}");
                allDayEvents = eventPool.GetRegularEvents(9);
                Debug.Log($"Generated {allDayEvents.Count} total events for the day");

                // Log all generated events for debugging
                for (int i = 0; i < allDayEvents.Count; i++)
                {
                    Debug.Log($"  Event {i + 1}: {allDayEvents[i].eventName}");
                }

                // Verify we got enough events
                if (allDayEvents.Count < 9)
                {
                    Debug.LogWarning($"Only got {allDayEvents.Count} regular events! Need 9 for a full day.");
                }
            }

            // Show the next 3 events based on current phase
            int startIndex = currentEventPhase * 3;
            currentDailyEvents.Clear();

            for (int i = 0; i < 3; i++)
            {
                int index = startIndex + i;
                if (index < allDayEvents.Count)
                {
                    currentDailyEvents.Add(allDayEvents[index]);
                }
                else
                {
                    Debug.LogError($"Not enough events! Phase {currentEventPhase}, index {index} out of {allDayEvents.Count}");
                }
            }

            Debug.Log($"Regular Phase {currentEventPhase + 1}/3: Showing 3 choices:");
            foreach (var ev in currentDailyEvents)
            {
                Debug.Log($"  - {ev.eventName}");
            }
        }

        Debug.Log($"=== GenerateDailyEvents END ===");
    }

    public PermanentStats GetPermanentStatsForUnit(Guid guid)
    {
        if (guid == null)
        {
            Debug.LogError("GetPermanentStatsForUnit called with null guid");
            return new PermanentStats(); // fail-safe
        }

        if (!permanentStatsMap.TryGetValue(guid, out var stats))
        {
            stats = new PermanentStats();
            permanentStatsMap[guid] = stats;
        }

        return stats;
    }

    public PermanentStats CreatePermanentStatsForUnit(Guid id)
    {
        var stats = new PermanentStats();
        permanentStatsMap[id] = stats;
        return stats;
    }

    public void InitializeShop(int count, Region region, UnitTagFlags unitTags, int minProvision = 0, int maxProvision = -1, bool forceRarity = false, Rarity designatedRarity = Rarity.Common)
    {
        if (shopState != null) return;

        shopState = new ShopState
        {
            // Pass the new rarity parameters into the service
            offeredUnits = UnitGenerationService.GenerateShopUnits(count, region, unitTags, minProvision, maxProvision, forceRarity, designatedRarity),
            purchasedUnits = new(),
            currentPage = 0,
            hasRefreshed = false,
            minProvisionFilter = minProvision,
            maxProvisionFilter = maxProvision,
        };
    }


    public Rarity RollRarityForDay(int day)
    {
        if (rarityDistributionTable == null)
        {
            return Rarity.Common; // fallback
        }

        var dist = rarityDistributionTable.GetForDay(day);
        if (dist == null)
        {
            return Rarity.Common;
        }

        return RarityDistributionTable.RollRarity(dist);
    }

    public void ResetRun()
    {
        Debug.Log("=== RESETTING RUN ===");

        Stats.Initialize();
        masterUnitStats.Clear();
        regularEventsCompleted = 0;
        isBattlePhase = false;
        currentEventPhase = 0;

        // Clear events
        currentDailyEvents.Clear();
        allDayEvents.Clear();
        selectedEvent = null;
        currentEncounter = null;
        eventInProgress = false;

        permanentStatsMap.Clear();

        // Reset team to default
        playerTeamPlacements.Clear();
        InitializeDefaultTeam();

        // Reset bench
        playerBenchPlacements.Clear();
        InitializeBench();
        AssignRegionTree();
        playerTactics.Clear();

        // Clear any other run-specific data
        Debug.Log("Run reset complete!");
    }
}
