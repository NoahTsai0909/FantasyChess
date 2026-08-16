using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "TacticDatabase", menuName = "Database/Tactic Database")]
public class TacticDatabase : ScriptableObject
{
    public List<TacticDefinition> allTactics = new List<TacticDefinition>();

    private static TacticDatabase _instance;
    public static TacticDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<TacticDatabase>("TacticDatabase");
                if (_instance == null)
                {
                    Debug.LogError("TacticDatabase not found in Resources folder!");
                }
            }
            return _instance;
        }
    }

    public TacticDefinition GetRandomTactic(Rarity rolledRarity, Region? region = null, bool byPassExclusivity = false)
    {
        IEnumerable<TacticDefinition> pool = allTactics;

        // Rarity eligibility rule
        pool = pool.Where(t => t.startingRarity <= rolledRarity);

        if (!byPassExclusivity)
        {
            pool = pool.Where(t => t.isEventExclusive == false);
        }
        else
        {
            pool = pool.Where(t => t.isEventExclusive == true);
        }

        // NEW: Check if the tactic's regions list contains the requested region
        if (region.HasValue)
        {
            pool = pool.Where(t => t.regions.Contains(region.Value));
        }

        var list = pool.ToList();
        if (list.Count == 0) return null;

        return list[Random.Range(0, list.Count)];
    }

    public List<TacticDefinition> GetTacticsByRegion(Region region)
    {
        return allTactics.Where(t => t.regions.Contains(region)).ToList();
    }

    private List<TacticDefinition> GetRandomFromList(List<TacticDefinition> source, int count)
    {
        if (source == null || source.Count == 0) return new List<TacticDefinition>();

        // Fisher-Yates shuffle
        for (int i = source.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (source[i], source[j]) = (source[j], source[i]);
        }

        int take = Mathf.Min(count, source.Count);
        return source.GetRange(0, take);
    }

    public List<TacticDefinition> GetRandomTactics(int count, Region? region = null)
    {
        IEnumerable<TacticDefinition> pool = allTactics;

        if (region.HasValue)
        {
            pool = pool.Where(t => t.regions.Contains(region.Value));
        }

        return GetRandomFromList(pool.ToList(), count);
    }
}