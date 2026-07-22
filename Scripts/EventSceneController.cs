using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;

public class EventSceneController : MonoBehaviour
{
    [Header("Basic UI")]
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private SpriteRenderer eventBackgroundRenderer;

    [Header("Dynamic Content")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button choiceButtonPrefab;

    private BaseEventSO currentEvent;

    // We now track a LIST of previews, since there can be multiple on screen
    private List<UnitInstance> spawnedPreviews = new();

    void Start()
    {
        currentEvent = RunManager.Instance.selectedEvent;

        if (currentEvent == null)
        {
            SceneLoader.Instance.LoadScene(GameScene.MapScene);
            return;
        }

        eventNameText.text = currentEvent.eventName;
        descriptionText.text = currentEvent.description;

        if (eventBackgroundRenderer != null && currentEvent.eventBackgroundImage != null)
            eventBackgroundRenderer.sprite = currentEvent.eventBackgroundImage;

        LoadEventChoices();
    }

    private void LoadEventChoices()
    {
        if (currentEvent is StoryEventSO storyEvent)
        {
            descriptionText.text = storyEvent.promptText;
            foreach (EventChoice choice in storyEvent.choices)
            {
                Button newButton = Instantiate(choiceButtonPrefab, contentParent);

                TextMeshProUGUI btnText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = choice.buttonText;

                // NEW: Create a context for this specific choice
                EventContext choiceContext = new EventContext();

                // 1. Check if we need to generate a completely random unit
                if (choice.generateRandomUnitPreview)
                {
                    // CHANGE: Pass choice.preferredTags instead of UnitTagFlags.None
                    UnitSaveData randomData = UnitGenerationService.GenerateUnit(
                        choice.randomRegion,
                        choice.preferredTags
                    );

                    choiceContext.generatedUnit = randomData;
                    SpawnUnitOnButton(randomData, newButton);
                }
                // 2. Otherwise, check if we have a specific unit to preview
                else if (choice.previewUnit != null)
                {
                    int day = RunManager.Instance.Stats.CurrentDay;
                    DayRarityEntry dist = RunManager.Instance.rarityDistributionTable.GetForDay(day);
                    Rarity rolledRarity = RarityDistributionTable.RollRarity(dist);

                    UnitSaveData generatedData = new UnitSaveData
                    {
                        definition = choice.previewUnit,
                        rarity = rolledRarity
                    };

                    choiceContext.generatedUnit = generatedData;
                    SpawnUnitOnButton(generatedData, newButton);
                }

                // Pass the unique context into the click event
                newButton.onClick.AddListener(() => ExecutePlayerChoice(choice, choiceContext));
            }
        }
    }

    // Notice we pass UnitSaveData now instead of UnitDefinition
    private void SpawnUnitOnButton(UnitSaveData unitData, Button parentButton)
    {
        Transform anchor = parentButton.transform.Find("UnitAnchor");
        if (anchor == null) return;

        UnitInstance preview = Instantiate(unitData.definition.unitPrefab, anchor);

        // The preview is now initialized with the real rolled rarity
        preview.InitializeFromSaveData(unitData);
        preview.isPlayer = true;
        preview.enabled = false;

        preview.transform.localPosition = Vector3.zero;
        preview.transform.localScale = Vector3.one * 25f;

        SpriteRenderer renderer = preview.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.sortingOrder = 100;

        spawnedPreviews.Add(preview);
    }

    private void ExecutePlayerChoice(EventChoice selectedChoice, EventContext context)
    {
        if (selectedChoice.outcome != null)
        {
            // Pass the context into the logic!
            selectedChoice.outcome.ExecuteOutcome(context);
        }
        CompleteEvent();
    }

    public void CompleteEvent()
    {
        // Clean up all spawned previews
        foreach (var preview in spawnedPreviews)
        {
            if (preview != null) Destroy(preview.gameObject);
        }
        spawnedPreviews.Clear();

        currentEvent.OnCompleted();
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}