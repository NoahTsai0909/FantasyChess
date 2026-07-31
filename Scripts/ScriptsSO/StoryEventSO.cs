using System.Collections.Generic;
using UnityEngine;
using static SceneLoader;

[CreateAssetMenu(fileName = "NewStoryEvent", menuName = "Events/Story Event")]
public class StoryEventSO : BaseEventSO
{
    [Header("Event Choices")]
    public string promptText;
    public List<EventChoice> choices;

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
            // This clears the old events and increments your 1/3 event counter
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

    [Tooltip("Check this to ignore previewUnit and generate a random unit instead")]
    public bool generateRandomUnitPreview;
    public Region randomRegion; // For the generation service
    public UnitTagFlags preferredTags = UnitTagFlags.None;

    public EventOutcomeSO outcome;
    public ChoiceConditionSO condition;
}
