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

    [Header("Availability")]
    public int minDayRequired;
    public int maxDayRequired;
    public float selectionWeight = 1.0f;

    [Header("Scene Management")]
    public GameScene targetScene;

    // Polymorphism: Let the children decide what happens!
    public virtual void OnSelected()
    {
        Debug.Log($"Event selected: {eventName}");
        RunManager.Instance.selectedEvent = this;
        RunManager.Instance.eventInProgress = true;

        // Children will handle specific setup, then call base.OnSelected() 
        // to finally load the scene.
        SceneLoader.Instance.LoadScene(targetScene);
    }

    public virtual void OnCompleted()
    {
        Debug.Log($"Event completed: {eventName}");
        RunManager.Instance.eventInProgress = false;
        RunManager.Instance.selectedEvent = null;

        // UI Updates can stay here
        if (MapController.Instance != null)
        {
            MapController.Instance.DisplayEvents(RunManager.Instance.currentDailyEvents);
            MapController.Instance.UpdateUI();
        }
    }

    public virtual bool IsAvailable()
    {
        int day = RunManager.Instance.Stats.CurrentDay;
        return day >= minDayRequired && day <= maxDayRequired;
    }
}