using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventPoolManager : MonoBehaviour
{
    public static EventPoolManager Instance { get; private set; }

    [Header("Event Pools")]
    [SerializeField] private List<BaseEventSO> allEvents = new List<BaseEventSO>();

    // Separate pools for faster filtering
    private List<BaseEventSO> combatEvents = new List<BaseEventSO>();
    private List<BaseEventSO> regularEvents = new List<BaseEventSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CategorizeEvents();
        DebugEventPool();
    }

    private void CategorizeEvents()
    {
        combatEvents.Clear();
        regularEvents.Clear();

        foreach (var eventSO in allEvents)
        {
            if (eventSO.IsAvailable())
            {
                if (eventSO is CombatEventSO)
                    combatEvents.Add(eventSO);
                else
                    regularEvents.Add(eventSO);
            }
        }

        Debug.Log($"Categorized: {combatEvents.Count} combat, {regularEvents.Count} regular events");
    }

    // Get combat events (weighted random)
    public List<BaseEventSO> GetCombatEvents(int count)
    {
        CategorizeEvents();
        return GetWeightedRandomEvents(combatEvents, count);
    }

    // Get regular events (weighted random)
    public List<BaseEventSO> GetRegularEvents(int count)
    {
        CategorizeEvents();
        return GetWeightedRandomEvents(regularEvents, count);
    }

    // Get any events (for backward compatibility)
    public List<BaseEventSO> GetRandomEvents(int count)
    {
        var availableEvents = combatEvents.Concat(regularEvents).ToList();
        return GetWeightedRandomEvents(availableEvents, count);
    }

    private List<BaseEventSO> GetWeightedRandomEvents(List<BaseEventSO> pool, int count)
    {
        if (pool.Count == 0) return new List<BaseEventSO>();

        List<BaseEventSO> selectedEvents = new List<BaseEventSO>();
        List<BaseEventSO> tempPool = new List<BaseEventSO>(pool);

        // If we need more events than available, allow duplicates
        bool allowDuplicates = (tempPool.Count < count);

        for (int i = 0; i < count; i++)
        {
            if (tempPool.Count == 0)
            {
                if (allowDuplicates)
                {
                    // Refill from original pool
                    tempPool = new List<BaseEventSO>(pool);
                }
                else
                {
                    break; // Not enough unique events
                }
            }

            // Weighted random selection
            BaseEventSO selected = SelectWeightedRandom(tempPool);
            selectedEvents.Add(selected);

            if (!allowDuplicates)
                tempPool.Remove(selected);
        }

        return selectedEvents;
    }

    private BaseEventSO SelectWeightedRandom(List<BaseEventSO> pool)
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var eventSO in pool)
        {
            totalWeight += eventSO.selectionWeight;
        }

        // Pick random point
        float randomPoint = Random.Range(0f, totalWeight);

        // Find which event this corresponds to
        float currentWeight = 0f;
        foreach (var eventSO in pool)
        {
            currentWeight += eventSO.selectionWeight;
            if (currentWeight >= randomPoint)
                return eventSO;
        }

        // Fallback
        return pool[0];
    }

    public void DebugEventPool()
    {
        Debug.Log("=== EVENT POOL DEBUG ===");
        Debug.Log($"Total events in pool: {allEvents.Count}");

        int combatCount = 0;
        int regularCount = 0;

        foreach (var eventSO in allEvents)
        {
            if (eventSO == null)
            {
                Debug.LogWarning("Found null event in pool!");
                continue;
            }

            string type = (eventSO is CombatEventSO) ? "COMBAT" : "REGULAR";
            bool available = eventSO.IsAvailable();

            Debug.Log($"- {eventSO.name}: {type}, Available: {available}, Weight: {eventSO.selectionWeight}");

            if (eventSO is CombatEventSO)
                combatCount++;
            else
                regularCount++;
        }

        Debug.Log($"Summary: {combatCount} combat, {regularCount} regular events");
        Debug.Log($"Categorized: {combatEvents.Count} combat, {regularEvents.Count} regular (after filtering)");
        Debug.Log("======================");
    }

    // Call this when reputation changes
    public void OnReputationChanged()
    {
        CategorizeEvents();
    }
}
