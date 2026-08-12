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
    [SerializeField] private Image eventBackgroundRenderer;

    [Header("Dynamic Content")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private Transform eventSpriteAnchor;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Unit Selector UI")]
    [SerializeField] private GameObject unitSelectorPanel;
    [SerializeField] private Transform unitSelectorContentParent;
    [SerializeField] private Button closeSelectorButton;

    private BaseEventSO currentEvent;

    // We now track a LIST of previews, since there can be multiple on screen
    private List<UnitInstance> spawnedPreviews = new();
    private GameObject spawnedIllustration;

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
            // The illustration usually stays the same across pages, so load it once here
            if (storyEvent.eventIllustration != null)
            {
                SpawnEventIllustration(storyEvent.eventIllustration);
            }

            // Start the event on the first page
            LoadEventPage(0);
        }
    }

    public void LoadEventPage(int pageIndex)
    {
        if (currentEvent is StoryEventSO storyEvent)
        {
            if (pageIndex < 0 || pageIndex >= storyEvent.pages.Count)
            {
                Debug.LogError($"Event {storyEvent.eventName} does not have a page at index {pageIndex}!");
                return;
            }

            EventPage currentPage = storyEvent.pages[pageIndex];

            // 1. Update narrative text
            descriptionText.text = currentPage.promptText;

            // 2. Clear old buttons and old previews
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
            // Ensure we destroy the physical unit instances so they don't pile up in memory
            foreach (var preview in spawnedPreviews)
            {
                if (preview != null) Destroy(preview.gameObject);
            }
            spawnedPreviews.Clear();

            // 3. Spawn new choices
            foreach (EventChoice choice in currentPage.choices)
            {
                Button newButton = Instantiate(choiceButtonPrefab, contentParent);
                TextMeshProUGUI btnText = newButton.GetComponentInChildren<TextMeshProUGUI>();

                string displayText = choice.buttonText;
                bool isInteractable = true;

                // Evaluate conditions
                if (choice.condition != null)
                {
                    isInteractable = choice.condition.IsMet();
                    if (!isInteractable)
                    {
                        displayText += $"\n<size=70%><color=#FF4444>({choice.condition.GetRequirementText()})</color></size>";
                    }
                }

                if (btnText != null) btnText.SetText(TextIconUtility.ParseDescription(displayText));
                newButton.interactable = isInteractable;

                EventContext choiceContext = new EventContext();
                choiceContext.uiController = this; // NEW: Pass a reference to this UI controller into the context!

                if (choice.generateRandomUnitPreview)
                {
                    UnitSaveData randomData = UnitGenerationService.GenerateUnit(choice.randomRegion, choice.preferredTags);
                    choiceContext.generatedUnit = randomData;
                    SpawnUnitOnButton(randomData, newButton);
                }
                else if (choice.previewUnit != null)
                {
                    Rarity finalRarity;

                    // Check the new flag to determine how we get the rarity
                    if (choice.rollRandomRarity)
                    {
                        int day = RunManager.Instance.Stats.CurrentDay;
                        DayRarityEntry dist = RunManager.Instance.rarityDistributionTable.GetForDay(day);
                        finalRarity = RarityDistributionTable.RollRarity(dist);
                    }
                    else
                    {
                        finalRarity = choice.previewRarity;
                    }

                    UnitSaveData generatedData = new UnitSaveData
                    {
                        definition = choice.previewUnit,
                        rarity = finalRarity
                    };

                    choiceContext.generatedUnit = generatedData;
                    SpawnUnitOnButton(generatedData, newButton);
                }

                newButton.onClick.AddListener(() => ExecutePlayerChoice(choice, choiceContext));
            }
        }
    }

    private void ExecutePlayerChoice(EventChoice selectedChoice, EventContext context)
    {
        if (selectedChoice.outcomes != null && selectedChoice.outcomes.Count > 0)
        {
            foreach (var outcome in selectedChoice.outcomes)
            {
                if (outcome != null)
                {
                    outcome.ExecuteOutcome(context);
                }
            }
            if (context.keepEventOpen)
            {
                return;
            }
        }

        CompleteEvent();
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
        if (preview.Visuals != null) preview.Visuals.SetBaseScale(preview.transform.localScale);

        SpriteRenderer renderer = preview.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.sortingOrder = 100;

        spawnedPreviews.Add(preview);
    }

    private void SpawnEventIllustration(Sprite artwork)
    {
        // 1. Create a new empty GameObject and name it
        spawnedIllustration = new GameObject("StoryIllustration");

        // 2. Parent it to your new anchor
        spawnedIllustration.transform.SetParent(eventSpriteAnchor, false);

        // 3. Add a SpriteRenderer and assign the artwork
        SpriteRenderer renderer = spawnedIllustration.AddComponent<SpriteRenderer>();
        renderer.sprite = artwork;

        // 4. Ensure it renders properly on your Screen Space - Camera canvas
        // (Set this to a number lower than 100 so unit previews still render on top of it)
        renderer.sortingOrder = 50;

        spawnedIllustration.transform.localPosition = Vector3.zero;
        //universally scale these images up or down
        spawnedIllustration.transform.localScale = Vector3.one * 10f;
    }

    // Notice the new parameters!
    public void ShowUnitSelectorPanel(UnitTargetEffectSO effectToApply, EventOutcomeSO onSuccessOutcome, EventContext context)
    {
        unitSelectorPanel.SetActive(true);

        // Hide the main event choices so their sprites don't bleed through!
        contentParent.gameObject.SetActive(false);

        // 1. Clear old buttons from the selector panel
        foreach (Transform child in unitSelectorContentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Setup a cancel button in case they change their mind
        closeSelectorButton.onClick.RemoveAllListeners();
        closeSelectorButton.onClick.AddListener(() =>
        {
            unitSelectorPanel.SetActive(false);
            contentParent.gameObject.SetActive(true); // Bring the choices back if they cancel!
        });

        // 3. Helper action to populate the list
        System.Action<RunManager.UnitPlacement> CreateUnitButton = (placement) =>
        {
            // Strictly ensure the unit definition actually exists before building the button
            if (placement == null || placement.unitData == null || placement.unitData.definition == null) return;

            Button newButton = Instantiate(choiceButtonPrefab, unitSelectorContentParent);
            TextMeshProUGUI btnText = newButton.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
                btnText.text = $"Select {placement.unitData.definition.unitName} (Tier {placement.unitData.rarity})";

            // Spawn the visual preview using your existing method
            SpawnUnitOnButton(placement.unitData, newButton);

            // Grab the sprite we just spawned and force it to render ABOVE the panel!
            SpriteRenderer sr = newButton.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 205;

            // When clicked, apply the effect, grant the reward, and finish!
            newButton.onClick.AddListener(() =>
            {
                // 1. Apply the Sacrifice
                effectToApply.ApplyEffect(placement);

                // 2. Grant the Cultist Reward (if one exists)
                if (onSuccessOutcome != null)
                {
                    context.keepEventOpen = false; // Allow the reward to close the event if needed
                    onSuccessOutcome.ExecuteOutcome(context);
                }

                // 3. Clean up
                unitSelectorPanel.SetActive(false);
                CompleteEvent();
            });
        };

        // 4. Populate Battle Grid Units
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            CreateUnitButton(placement);
        }

        // 5. Populate Bench Units
        foreach (var placement in RunManager.Instance.playerBenchPlacements)
        {
            CreateUnitButton(placement);
        }
    }


    public void CompleteEvent()
    {
        spawnedPreviews.Clear();

        spawnedIllustration = null;

        currentEvent.OnCompleted();
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}