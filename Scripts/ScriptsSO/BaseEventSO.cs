using UnityEngine;
using static SceneLoader;

public abstract class BaseEventSO : ScriptableObject
{
    [Header("Basic Info")]
    public string eventName;
    [TextArea(3, 5)]
    public string description;
    public Sprite eventIcon;
    public Sprite eventBackgroundImage;
    public string eventButtonText;

    [Header("Availability")]
    public int minDayRequired;
    public int maxDayRequired;
    public float selectionWeight = 1.0f; // For weighted random

    [Header("Scene Management")]
    public GameScene targetScene; // Which scene loads for this event


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
        else if (this is ShopEventSO shopEvent)
        {
            targetScene = GameScene.ShopScene;
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

        bool isCombat = (this is CombatEventSO);

        if (isCombat)
        {
            RunManager.Instance.CompleteBattleEvent();
        }
        else
        {
            RunManager.Instance.CompleteRegularEvent();
        }

        // Update UI
        if (MapController.Instance != null)
        {
            MapController.Instance.DisplayEvents(RunManager.Instance.currentDailyEvents);
            MapController.Instance.UpdateUI();
        }
    }

    public virtual bool IsAvailable()
    {
        int day = RunManager.Instance.currentDay;
        return day >= minDayRequired && day <= maxDayRequired;
    }

    public virtual UnitSaveData ReturnRandomUnit()
    {
        return null;
    }

    public virtual int getGoldAmount()
    {
        return 0;
    }
}


