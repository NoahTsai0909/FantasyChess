using System.Collections.Generic;
using UnityEngine;
using static SceneLoader;

[System.Serializable]
public struct EventPage
{
    [TextArea(3, 5)]
    public string promptText;
    public List<EventChoice> choices;
}

[CreateAssetMenu(fileName = "NewStoryEvent", menuName = "Events/Story Event")]
public class StoryEventSO : BaseEventSO
{
    [Header("Event Pages (Node System)")]
    [Tooltip("Page 0 is always the starting page. Use outcomes to navigate to other indices.")]
    public List<EventPage> pages = new List<EventPage>();

    [Header("Event Visuals")]
    public Sprite eventIllustration;

    public override void OnSelected()
    {
        targetScene = GameScene.EventScene;
        base.OnSelected();
    }

    public override void OnCompleted()
    {
        if (RunManager.Instance != null)
        {
            // This clears the old events and increments your event counter
            RunManager.Instance.CompleteRegularEvent();
        }

        base.OnCompleted();
    }
}

[System.Serializable]
public class EventChoice
{
    public string buttonText;
    [Header("Preview Settings")]
    [Tooltip("The specific unit to preview next to this button (optional)")]
    public UnitDefinition previewUnit;
    public bool rollRandomRarity = true; // NEW FLAG
    public Rarity previewRarity;

    [Tooltip("Check this to ignore previewUnit and generate a random unit instead")]
    public bool generateRandomUnitPreview;
    public Region randomRegion;
    public UnitTagFlags preferredTags = UnitTagFlags.None;

    public EventOutcomeSO outcome;
    public ChoiceConditionSO condition;
}