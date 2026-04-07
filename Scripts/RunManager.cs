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
    public int currentGold = 10;

    public List<UnitPlacement> playerTeamPlacements = new();
    public List<UnitPlacement> playerBenchPlacements = new();

    [System.Serializable]
    public class UnitPlacement
    {
        public UnitSaveData unitData;
        public int row;
        public int col;
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
    [SerializeField] private int benchSize = 8;

    public int currentDay = 1;
    public int regularEventsCompleted = 0;
    public const int REGULAR_EVENTS_PER_DAY = 3;
    public bool isBattlePhase = false;
    public int currentEventPhase = 0;
    public int reputation = 0;
    public int playerLevel = 1;
    public int playerHealth = 12;

    // New: Day event tracking
    public List<BaseEventSO> currentDailyEvents = new();  // Current 3 events to display
    public List<BaseEventSO> allDayEvents = new();

    public BaseEventSO selectedEvent;
    public EncounterDefinition currentEncounter;
    public bool eventInProgress = false;
    public const int TOTAL_DAYS = 12;
    public ShopState shopState;
    public int provisionCap = 4;

    private Dictionary<Guid, PermanentStats> permanentStatsMap = new();

    [SerializeField] public RarityDistributionTable rarityDistributionTable;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (playerTeamPlacements.Count == 0)
                InitializeDefaultTeam();

            InitializeBench();
            SanitizeBench();

            regularEventsCompleted = 0;
            isBattlePhase = false;
            currentEventPhase = 0;
        }
        else
        {
            Destroy(gameObject);
        }
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
                Debug.LogWarning($"Sanitizing invalid bench slot {i}");
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
            if (placement.unitData == null)
                continue;

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
        currentDay++;
        regularEventsCompleted = 0;
        isBattlePhase = false;
        currentEventPhase = 0;
        allDayEvents.Clear();
        currentDailyEvents.Clear();

        GenerateDailyEvents();
        Debug.Log($"Day {currentDay} started!");
    }

    public void CompleteRegularEvent()
    {
        regularEventsCompleted++;
        reputation++;
        Debug.Log($"Regular event completed: {regularEventsCompleted}/{REGULAR_EVENTS_PER_DAY}");

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

        Debug.Log($"Battle completed! Day {currentDay} is complete.");

        // Check if run is complete (7 days)
        if (currentDay >= TOTAL_DAYS)
        {
            Debug.Log($"RUN COMPLETE! Finished all {TOTAL_DAYS} days!");
            SceneLoader.Instance.LoadScene(GameScene.MainMenuScene);
            return;
        }

        // Start the next day
        StartNewDay();
    }


    public void GenerateDailyEvents()
    {
        Debug.Log($"=== GenerateDailyEvents START ===");
        Debug.Log($"Day {currentDay}, Phase: {(isBattlePhase ? "BATTLE" : $"REGULAR {currentEventPhase + 1}/3")}, Regular events completed: {regularEventsCompleted}");

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
                Debug.Log($"GENERATING ALL 9 REGULAR EVENTS FOR DAY {currentDay}");
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


    public void InitializeShop(int count, Region region, UnitTagFlags unitTags, int minProvision = 0, int maxProvision = -1)
    {
        if (shopState != null) return;

        shopState = new ShopState
        {
            offeredUnits = GenerateShopUnits(count, region, unitTags, minProvision, maxProvision),
            purchasedUnits = new(),
            currentPage = 0,
            hasRefreshed = false,
            minProvisionFilter = minProvision,
            maxProvisionFilter = maxProvision
        };
    }

    private List<UnitSaveData> GenerateShopUnits(int count, Region region, UnitTagFlags unitTags, int minProvision = 0, int maxProvision = -1)
    {
        var result = new List<UnitSaveData>();
        var usedDefinitions = new HashSet<UnitDefinition>();

        for (int i = 0; i < count; i++)
        {
            var rarity = RollRarityForDay(currentDay);

            UnitDefinition def = null;
            int attempts = 0;

            while (def == null && attempts < 100)
            {
                attempts++;

                var candidate = UnitDatabase.Instance.GetRandomUnit(
                    rarity,
                    region,
                    unitTags,
                    minProvision,  // Add provision filtering
                    maxProvision
                );

                if (candidate != null && !usedDefinitions.Contains(candidate))
                {
                    def = candidate;
                }
            }

            if (def != null)
            {
                usedDefinitions.Add(def);
                result.Add(new UnitSaveData
                {
                    definition = def,
                    rarity = rarity
                });

                Debug.Log($"Generated: {def.unitName} (Provision: {def.provisionCost}, Rarity: {rarity})");
            }
            else
            {
                Debug.LogWarning($"Could not find unique unit for rarity {rarity} with provision {minProvision}-{maxProvision}");
            }
        }

        return result;
    }


    public Rarity RollRarityForDay(int day)
    {
        if (rarityDistributionTable == null)
        {
            Debug.LogError("RarityDistributionTable not assigned!");
            return Rarity.Common; // fallback
        }

        var dist = rarityDistributionTable.GetForDay(day);
        if (dist == null)
        {
            Debug.LogError($"No rarity distribution for day {day}");
            return Rarity.Common;
        }

        return RarityDistributionTable.RollRarity(dist);
    }


    public void ResetRun()
    {
        Debug.Log("=== RESETTING RUN ===");

        currentGold = 10;
        currentDay = 1;
        provisionCap = 4;
        regularEventsCompleted = 0;
        isBattlePhase = false;
        reputation = 0;
        currentEventPhase = 0;
        playerHealth = 12;

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


        // Clear any other run-specific data
        Debug.Log("Run reset complete!");
    }
}
