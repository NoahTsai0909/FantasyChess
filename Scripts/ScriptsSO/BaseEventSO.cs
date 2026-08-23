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
    [Header("Visuals")]
    public Color portalGlowColor = Color.white;

    [Header("Availability")]
    public int minDayRequired;
    public int maxDayRequired;
    public float selectionWeight = 1.0f;

    [Tooltip("Max times this event can appear on the map per run. (0 = infinite)")]
    public int maxAppearancesPerRun = 0;

    [Header("Scene Management")]
    public GameScene targetScene;

    public virtual void OnSelected()
    {
        Debug.Log($"Event selected: {eventName}");
        RunManager.Instance.selectedEvent = this;
        RunManager.Instance.eventInProgress = true;

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
        bool isDayValid = day >= minDayRequired && day <= maxDayRequired;

        if (maxAppearancesPerRun > 0 && EventPoolManager.Instance != null)
        {
            int currentCount = EventPoolManager.Instance.GetAppearanceCount(name);
            if (currentCount >= maxAppearancesPerRun)
            {
                return false;
            }
        }

        return isDayValid;
    }
}