using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventPoolManager : MonoBehaviour
{
    public static EventPoolManager Instance { get; private set; }

    private List<BaseEventSO> allEvents = new List<BaseEventSO>();

    // Separate pools for faster filtering
    private List<BaseEventSO> combatEvents = new List<BaseEventSO>();
    private List<BaseEventSO> regularEvents = new List<BaseEventSO>();

    private Dictionary<string, int> eventAppearanceCounts = new Dictionary<string, int>();

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
        LoadEventsFromResources();
        CategorizeEvents();
        DebugEventPool();
    }

    private void LoadEventsFromResources()
    {
        // This automatically finds EVERY BaseEventSO inside Assets/Resources/Events/
        BaseEventSO[] loadedEvents = Resources.LoadAll<BaseEventSO>("Events");

        // Convert the array to list
        allEvents = new List<BaseEventSO>(loadedEvents);
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

    public List<BaseEventSO> GetRandomEvents(int count)
    {
        CategorizeEvents();
        var availableEvents = combatEvents.Concat(regularEvents).ToList();
        return GetWeightedRandomEvents(availableEvents, count);
    }

    private List<BaseEventSO> GetWeightedRandomEvents(List<BaseEventSO> pool, int count)
    {
        if (pool.Count == 0) return new List<BaseEventSO>();

        List<BaseEventSO> selectedEvents = new List<BaseEventSO>();
        List<BaseEventSO> tempPool = new List<BaseEventSO>(pool);

        bool allowDuplicates = (tempPool.Count < count);

        for (int i = 0; i < count; i++)
        {
            if (tempPool.Count == 0)
            {
                if (allowDuplicates)
                {
                    tempPool = new List<BaseEventSO>(pool);
                }
                else
                {
                    break;
                }
            }

            // Weighted random selection
            BaseEventSO selected = SelectWeightedRandom(tempPool);
            selectedEvents.Add(selected);

            RecordEventAppearance(selected.name);

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
    }

    // Call this when reputation changes
    public void OnReputationChanged()
    {
        CategorizeEvents();
    }

    public int GetAppearanceCount(string eventName)
    {
        if (eventAppearanceCounts.TryGetValue(eventName, out int count)) return count;
        return 0;
    }

    public void RecordEventAppearance(string eventName)
    {
        if (eventAppearanceCounts.ContainsKey(eventName))
            eventAppearanceCounts[eventName]++;
        else
            eventAppearanceCounts[eventName] = 1;
    }


}
