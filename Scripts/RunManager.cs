using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static SceneLoader;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    // Run Data
    public int currentGold = 100;
    public int currentNodeIndex = 0;

    public List<UnitPlacement> playerTeamPlacements = new List<UnitPlacement>();
    public List<UnitPlacement> playerBenchPlacements= new List<UnitPlacement>();

    [System.Serializable]
    public class UnitPlacement
    {
        public UnitInstance unitPrefab;
        public int row;
        public int col;
    }

    [Header("Default Unit")]
    [SerializeField] private List<UnitPlacement> defaultUnits;

    public int currentDay = 1;
    public int regularEventsCompleted = 0;
    public const int REGULAR_EVENTS_BEFORE_BATTLE = 3;
    public bool isBattleDay = false;
    public int reputation = 1;
    public List<BaseEventSO> currentDailyEvents = new List<BaseEventSO>();
    public BaseEventSO selectedEvent;
    public EncounterDefinition currentEncounter;
    public bool eventInProgress = false;
    public const int TOTAL_DAYS = 7;

    private Dictionary<UnitInstance, PermanentStats> permanentStatsMap
    = new Dictionary<UnitInstance, PermanentStats>();

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

            InitializeBench();

            // Initialize progression tracking
            regularEventsCompleted = 0; // Ensure it starts at 0
            isBattleDay = false; // Start with regular events
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDefaultTeam()
    {
        playerTeamPlacements = defaultUnits;


    }

    private void InitializeBench()
    {
        // If bench is empty, create 8 empty slots (typical autobattler bench size)
        if (playerBenchPlacements.Count == 0)
        {
            for (int i = 0; i < 8; i++) // 8 bench slots
            {
                playerBenchPlacements.Add(new UnitPlacement
                {
                    unitPrefab = null,  // Empty slot
                    row = 0,            // Bench doesn't use grid positioning
                    col = i             // Index as column for organization
                });
            }
            Debug.Log("Initialized empty bench with 8 slots");
        }
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

    public TeamDefinition GetTeamForBench()
    {
        TeamDefinition benchTeam = ScriptableObject.CreateInstance<TeamDefinition>();
        benchTeam.teamName = "Player Bench";
        
        foreach ( var placement in playerBenchPlacements)
        {
            benchTeam.units.Add(new TeamDefinition.UnitPlacement
            {
                unitPrefab = placement.unitPrefab,
                row = placement.row,
                col = placement.col
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

    public void AddUnitToBench(UnitDefinition unitDefinition)
    {
        if (unitDefinition == null || unitDefinition.unitPrefab == null)
        {
            Debug.LogError("Cannot add null unit or unit without prefab to bench!");
            return;
        }

        // Find an empty bench spot
        for (int i = 0; i < playerBenchPlacements.Count; i++)
        {
            if (playerBenchPlacements[i].unitPrefab == null)
            {
                playerBenchPlacements[i].unitPrefab = unitDefinition.unitPrefab;
                Debug.Log($"Added {unitDefinition.unitName} to bench slot {i}");

                // Optional: Log bench status
                LogBenchStatus();
                return;
            }
        }

        Debug.LogWarning($"No space on bench for {unitDefinition.unitName}! Bench is full.");
        LogBenchStatus();
    }

    private void LogBenchStatus()
    {
        int emptySlots = 0;
        int filledSlots = 0;

        foreach (var slot in playerBenchPlacements)
        {
            if (slot.unitPrefab == null)
                emptySlots++;
            else
                filledSlots++;
        }

        Debug.Log($"Bench: {filledSlots} filled, {emptySlots} empty slots");
    }

    public void AddRandomUnitToBench()
    {
        if (UnitDatabase.Instance != null)
        {
            UnitDefinition randomUnit = UnitDatabase.Instance.GetRandomUnit();
            if (randomUnit != null)
            {
                AddUnitToBench(randomUnit);
            }
        }
    }
    public PermanentStats GetPermanentStatsForUnit(UnitInstance unitKey)
    {
        if (!permanentStatsMap.TryGetValue(unitKey, out var stats))
        {
            stats = new PermanentStats();
            permanentStatsMap[unitKey] = stats;
        }
        return stats;
    }

    public StatBlock GetPreviewStats(UnitDefinition definition)
    {
        PermanentStats permStats = GetPermanentStatsForUnit(definition.unitPrefab);
        TemporaryStats tempStats = new TemporaryStats(); // or new TemporaryStats()
        return new StatBlock(definition, permStats, tempStats);
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
