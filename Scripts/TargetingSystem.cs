// TargetingSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem
{
    private GridManager playerGrid;
    private GridManager enemyGrid;
    private bool isPlayerTeam;

    public TargetingSystem(GridManager playerGrid, GridManager enemyGrid, bool isPlayerTeam)
    {
        this.playerGrid = playerGrid;
        this.enemyGrid = enemyGrid;
        this.isPlayerTeam = isPlayerTeam;
    }

    // Generic method to find units based on criteria
    public UnitInstance FindUnit(TargetCriteria criteria, Vector3 referencePosition = default)
    {
        List<UnitInstance> candidates = GetCandidates(criteria.targetTeam);

        if (candidates.Count == 0) return null;

        return criteria.sortMethod switch
        {
            SortMethod.Nearest => FindNearest(candidates, referencePosition),
            SortMethod.Farthest => FindFarthest(candidates, referencePosition),
            SortMethod.LowestHealth => FindLowestHealth(candidates),
            SortMethod.HighestHealth => FindHighestHealth(candidates),
            SortMethod.Random => FindRandom(candidates),
            _ => FindNearest(candidates, referencePosition)
        };
    }

    // Get all units from the target team
    private List<UnitInstance> GetCandidates(TargetTeam targetTeam)
    {
        GridManager grid = targetTeam switch
        {
            TargetTeam.Enemy => enemyGrid,
            TargetTeam.Ally => playerGrid,
            TargetTeam.Any => null,
            _ => null
        };

        if (grid == null)
        {
            // Combine both grids for "Any"
            var allUnits = new List<UnitInstance>();
            allUnits.AddRange(playerGrid.GetAllUnits());
            allUnits.AddRange(enemyGrid.GetAllUnits());
            return FilterByTeam(allUnits, targetTeam);
        }

        return FilterByTeam(grid.GetAllUnits(), targetTeam);
    }

    private List<UnitInstance> FilterByTeam(List<UnitInstance> units, TargetTeam targetTeam)
    {
        if (targetTeam == TargetTeam.Any) return units;
        bool shouldBePlayerTeam;
        if (targetTeam == TargetTeam.Enemy)
        {
            shouldBePlayerTeam = !isPlayerTeam;
        }
        else
        {
            shouldBePlayerTeam = isPlayerTeam;
        }
        return units.FindAll(unit => unit != null && unit.isPlayer == shouldBePlayerTeam);
    }

    // Individual finder methods
    private UnitInstance FindNearest(List<UnitInstance> candidates, Vector3 referencePosition)
    {
        UnitInstance nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var unit in candidates)
        {
            float distance = Vector3.Distance(referencePosition, unit.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = unit;
            }
        }
        return nearest;
    }

    private UnitInstance FindFarthest(List<UnitInstance> candidates, Vector3 referencePosition)
    {
        UnitInstance farthest = null;
        float farthestDistance = float.MinValue;

        foreach (var unit in candidates)
        {
            float distance = Vector3.Distance(referencePosition, unit.transform.position);
            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthest = unit;
            }
        }
        return farthest;
    }

    private UnitInstance FindLowestHealth(List<UnitInstance> candidates)
    {
        UnitInstance lowest = null;
        // 1. Start higher than 1 so full-health units can be selected
        float lowestHealth = float.MaxValue;

        foreach (var unit in candidates)
        {
            float health = unit.GetCurrentHP() / unit.Stats.MaxHP;

            // 2. Use <= so it defaults to picking the first candidate if everyone is tied
            if (health <= lowestHealth)
            {
                lowestHealth = health;
                lowest = unit;
            }
        }
        return lowest;
    }

    private UnitInstance FindHighestHealth(List<UnitInstance> candidates)
    {
        UnitInstance highest = null;
        float highestHealth = 0;

        foreach (var unit in candidates)
        {
            // You'll need to expose currentHP via a property
            float health = unit.GetCurrentHP(); // Need to add this method
            if (health > highestHealth)
            {
                highestHealth = health;
                highest = unit;
            }
        }
        return highest;
    }

    // Generic method to find multiple units based on criteria
    public List<UnitInstance> FindMultipleUnits(TargetCriteria criteria, int maxCount, Vector3 referencePosition = default)
    {
        List<UnitInstance> candidates = GetCandidates(criteria.targetTeam);

        if (candidates.Count == 0) return new List<UnitInstance>();

        return criteria.sortMethod switch
        {
            SortMethod.Nearest => FindNearestMultiple(candidates, maxCount, referencePosition),
            // You can add other multi-target sorts later like FindFarthestMultiple, FindRandomMultiple, etc.
            _ => FindNearestMultiple(candidates, maxCount, referencePosition)
        };
    }

    // Helper method to sort and grab the top X nearest candidates
    private List<UnitInstance> FindNearestMultiple(List<UnitInstance> candidates, int count, Vector3 referencePosition)
    {
        // Sort the candidates list by distance in ascending order
        candidates.Sort((a, b) =>
        {
            float distA = Vector3.Distance(referencePosition, a.transform.position);
            float distB = Vector3.Distance(referencePosition, b.transform.position);
            return distA.CompareTo(distB);
        });

        // Clamp the count so we don't try to grab more units than actually exist
        int returnCount = Mathf.Min(count, candidates.Count);
        return candidates.GetRange(0, returnCount);
    }



    private UnitInstance FindRandom(List<UnitInstance> candidates)
    {
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    // Enums for configuration
    public enum TargetTeam { Enemy, Ally, Any }
    public enum SortMethod { Nearest, Farthest, LowestHealth, HighestHealth, Random }

    public class TargetCriteria
    {
        public TargetTeam targetTeam;
        public SortMethod sortMethod;
        public int maxRange = int.MaxValue;

        public TargetCriteria(TargetTeam team, SortMethod method)
        {
            targetTeam = team;
            sortMethod = method;
        }
    }

    public List<UnitInstance> GetAllies()
    {
        return GetCandidates(TargetTeam.Ally);
    }

    public List<UnitInstance> GetEnemies()
    {
        return GetCandidates(TargetTeam.Enemy);
    }
}
