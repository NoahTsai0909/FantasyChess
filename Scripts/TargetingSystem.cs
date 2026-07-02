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
        float lowestHealth = 1;

        foreach (var unit in candidates)
        {
            // You'll need to expose currentHP via a property
            float health = unit.GetCurrentHP()/unit.Stats.MaxHP; // Need to add this method
            if (health < lowestHealth)
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
