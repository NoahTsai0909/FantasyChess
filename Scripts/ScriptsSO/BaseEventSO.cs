using UnityEngine;
using static SceneLoader;

public abstract class BaseEventSO : ScriptableObject
{
    [Header("Basic Info")]
    public string eventName;
    [TextArea(3, 5)]
    public string description;
    public Sprite eventIcon;

    [Header("Availability")]
    public int minReputationRequired = 1;
    public int maxReputationAllowed = 10;
    public float selectionWeight = 1.0f; // For weighted random

    [Header("Scene Management")]
    public GameScene targetScene; // Which scene loads for this event

    [Header("Rewards")]
    public Reward[] possibleRewards;

    [Header("Visuals")]
    public Color backgroundColor = Color.white;
    public string flavorText;

    // Called when player selects this event
    public virtual void OnSelected()
    {
        Debug.Log($"Event selected: {eventName}");
        RunManager.Instance.selectedEvent = this;
        RunManager.Instance.eventInProgress = true;

        // Store in RunManager if it's a combat event
        if (this is CombatEventSO combatEvent)
        {
            RunManager.Instance.currentEncounter = combatEvent.encounter;
            Debug.Log($"Stored encounter: {combatEvent.encounter?.encounterName}");
            targetScene = GameScene.CombatScene; // Combat events go to combat
        }
        else
        {
            targetScene = GameScene.EventScene; // Regular events go to event scene
        }

        // Load appropriate scene
        Debug.Log($"Loading scene: {targetScene}");
        SceneLoader.Instance.LoadScene(targetScene);
    }

    public void CompleteEvent()
    {
        OnCompleted();
    }

    // Called when event is completed
    public virtual void OnCompleted()
    {
        Debug.Log($"Event completed: {eventName}");
        RunManager.Instance.eventInProgress = false;
        RunManager.Instance.selectedEvent = null;

        // Apply rewards
        ApplyRandomReward();

        bool isCombat = (this is CombatEventSO);

        if (isCombat)
        {
            // BATTLE COMPLETED - increment day and reset
            RunManager.Instance.currentDay++;
            RunManager.Instance.CompleteBattleEvent();
            Debug.Log($"Battle completed! Day {RunManager.Instance.currentDay} complete.");
        }
        else
        {
            // REGULAR EVENT - just track
            RunManager.Instance.CompleteRegularEvent();
        }

        // Update UI
        if (MapController.Instance != null)
        {
            MapController.Instance.DisplayEvents(RunManager.Instance.currentDailyEvents);
            MapController.Instance.UpdateUI();
        }
    }

    protected virtual void ApplyRandomReward()
    {
        if (possibleRewards != null && possibleRewards.Length > 0)
        {
            Reward reward = possibleRewards[Random.Range(0, possibleRewards.Length)];
            reward.Apply();
        }
    }

    public virtual bool IsAvailable()
    {
        int rep = RunManager.Instance.reputation;
        return rep >= minReputationRequired && rep <= maxReputationAllowed;
    }
}

[System.Serializable]
public class Reward
{
    public RewardType type;
    public int value;
    public UnitDefinition unitReward; // For unit rewards
    public string customEffect; // For special effects

    public void Apply()
    {
        switch (type)
        {
            case RewardType.Gold:
                RunManager.Instance.currentGold += value;
                Debug.Log($"Reward: Gained {value} gold");
                break;
            case RewardType.Reputation:
                RunManager.Instance.reputation += value;
                Debug.Log($"Reward: Gained {value} reputation");
                break;
            case RewardType.RandomUnit:
                // Actually give a random unit
                if (UnitDatabase.Instance != null)
                {
                    UnitDefinition randomUnit = UnitDatabase.Instance.GetRandomUnit();
                    if (randomUnit != null)
                    {
                        RunManager.Instance.AddUnitToBench(randomUnit);
                        Debug.Log($"Reward: Gained random unit {randomUnit.unitName}");
                    }
                }
                break;
            case RewardType.SpecificUnit:
                if (unitReward != null)
                {
                    RunManager.Instance.AddUnitToBench(unitReward);
                    Debug.Log($"Reward: Gained specific unit {unitReward.unitName}");
                }
                break;
        }
    }
}

public enum RewardType
{
    Gold,
    Reputation,
    RandomUnit,
    SpecificUnit
}
