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
    }


    [Header("Default Unit")]
    [SerializeField] private List<UnitSaveData> defaultUnits;
    [SerializeField] private int benchSize = 8;

    public int currentDay = 1;
    public int regularEventsCompleted = 0;
    public const int REGULAR_EVENTS_BEFORE_BATTLE = 3;
    public bool isBattleDay = false;
    public int reputation = 1;
    public List<BaseEventSO> currentDailyEvents = new();
    public BaseEventSO selectedEvent;
    public EncounterDefinition currentEncounter;
    public bool eventInProgress = false;
    public const int TOTAL_DAYS = 7;
    public ShopState shopState;
    public int provisionCap = 4;

    private Dictionary<UnitDefinition, PermanentStats> permanentStatsMap = new();

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
            isBattleDay = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDefaultTeam()
    {
        playerTeamPlacements.Clear();

        foreach (var data in defaultUnits)
        {
            if (data == null || data.definition == null)
            {
                Debug.LogError("Default unit has null definition!");
                continue;
            }

            playerTeamPlacements.Add(new UnitPlacement
            {
                unitData = new UnitSaveData
                {
                    definition = data.definition,
                    rarity = data.rarity
                },
                row = data.row,
                col = data.col
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
        isBattleDay = (currentDay % 3 == 0); // Every 3rd day is battle

        GenerateDailyEvents();
    }

    public void CompleteRegularEvent()
    {
        regularEventsCompleted++;
        Debug.Log($"Regular event completed: {regularEventsCompleted}/{REGULAR_EVENTS_BEFORE_BATTLE}");

        // Check if it's time for a battle
        if (regularEventsCompleted >= REGULAR_EVENTS_BEFORE_BATTLE)
        {
            isBattleDay = true;
            regularEventsCompleted = 0; // Reset counter
            Debug.Log("3 regular events completed! Time for a battle!");
        }
        else
        {
            isBattleDay = false;
        }

        // Generate new events for next choice
        GenerateDailyEvents();
    }

    public void CompleteBattleEvent()
    {
        Debug.Log($"=== CompleteBattleEvent ===");

        // Battle completed, go back to regular events
        isBattleDay = false;
        regularEventsCompleted = 0; // <-- EXPLICITLY RESET THIS!
        Debug.Log($"Battle completed! Returning to regular events. Reset regularEventsCompleted to 0");

        // Check if run is complete (7 days)
        if (currentDay >= TOTAL_DAYS)
        {
            Debug.Log($"RUN COMPLETE! Finished all {TOTAL_DAYS} days!");
            SceneLoader.Instance.LoadScene(GameScene.MainMenuScene);
            return;
        }

        GenerateDailyEvents();
        Debug.Log($"=== END CompleteBattleEvent ===");
    }


    public void GenerateDailyEvents()
    {
        Debug.Log($"=== GenerateDailyEvents START ===");
        Debug.Log($"Parameters: isBattleDay={isBattleDay}, regularEventsCompleted={regularEventsCompleted}, currentDay={currentDay}");

        currentDailyEvents.Clear();

        EventPoolManager eventPool = EventPoolManager.Instance;

        if (eventPool != null)
        {
            Debug.Log($"EventPool found with {eventPool.GetRegularEvents(1).Count} regular and {eventPool.GetCombatEvents(1).Count} combat events available");

            if (isBattleDay)
            {
                Debug.Log("GENERATING COMBAT EVENTS (isBattleDay=true)");
                currentDailyEvents = eventPool.GetCombatEvents(3);
                Debug.Log($"Got {currentDailyEvents.Count} combat events");

                // Debug each event
                foreach (var ev in currentDailyEvents)
                {
                    Debug.Log($"  - {ev.eventName} ({(ev is CombatEventSO ? "Combat" : "Regular")})");
                }
            }
            else
            {
                Debug.Log("GENERATING REGULAR EVENTS (isBattleDay=false)");
                currentDailyEvents = eventPool.GetRegularEvents(3);
                Debug.Log($"Got {currentDailyEvents.Count} regular events");

                // Debug each event
                foreach (var ev in currentDailyEvents)
                {
                    Debug.Log($"  - {ev.eventName} ({(ev is CombatEventSO ? "Combat" : "Regular")})");
                }
            }
        }
        else
        {
            Debug.LogError("EventPoolManager.Instance is NULL!");
        }

        Debug.Log($"=== GenerateDailyEvents END: Generated {currentDailyEvents.Count} events ===");
    }

    public PermanentStats GetPermanentStatsForUnit(UnitDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogError("GetPermanentStatsForUnit called with null UnitDefinition");
            return new PermanentStats(); // fail-safe
        }

        if (!permanentStatsMap.TryGetValue(definition, out var stats))
        {
            stats = new PermanentStats();
            permanentStatsMap[definition] = stats;
        }

        return stats;
    }



    public StatBlock GetPreviewStats(UnitDefinition definition, Rarity rarity)
    {
        int delta = rarity - definition.startingRarity;
        float multiplier = RarityScaling.GetMultiplier(delta);

        IStatSource rarityAdjusted = new UnitDefinitionView(
            Mathf.RoundToInt(definition.attack * multiplier),
            Mathf.RoundToInt(definition.heal * multiplier),
            Mathf.RoundToInt(definition.maxHP * multiplier),
            definition.cooldown
        );

        PermanentStats permStats = new PermanentStats(); // no persistence for previews
        TemporaryStats tempStats = new TemporaryStats();

        return new StatBlock(rarityAdjusted, permStats, tempStats);
    }


    public void InitializeShop(int count, Region region, UnitTagFlags unitTags)
    {
        if (shopState != null) return;

        shopState = new ShopState
        {
            offeredUnits = GenerateShopUnits(count, region, unitTags), // NEW: Generate with rarities
            purchasedUnits = new(),
            currentPage = 0,
            hasRefreshed = false
        };
    }

    private List<UnitSaveData> GenerateShopUnits(int count, Region region, UnitTagFlags unitTags)
    {
        var result = new List<UnitSaveData>();

        // Keep track of used definitions to avoid duplicates
        var usedDefinitions = new HashSet<UnitDefinition>();

        for (int i = 0; i < count; i++)
        {
            // Roll rarity first
            var rarity = RollRarityForDay(currentDay);

            // Get a random unit that respects rarity, region, tags, AND hasn't been used
            UnitDefinition def = null;
            int attempts = 0;

            while (def == null && attempts < 100)
            {
                attempts++;

                // Try to get a random unit with the rolled rarity
                var candidate = UnitDatabase.Instance.GetRandomUnit(rarity, region, unitTags);

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

                Debug.Log($"Generated: {def.unitName} as {rarity}");
            }
            else
            {
                Debug.LogWarning($"Could not find unique unit for rarity {rarity}");
            }
        }

        return result;
    }

    private Rarity RollRarityForDay(int day)
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

        // Reset all run progression
        currentGold = 100;
        currentDay = 1;
        regularEventsCompleted = 0;
        isBattleDay = false;
        reputation = 1;

        // Clear events
        currentDailyEvents.Clear();
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
