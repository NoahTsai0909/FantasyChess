using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneLoader;
using TMPro;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI reputationText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI dayTypeText;
    [SerializeField] private TextMeshProUGUI eventCounterText;
    [SerializeField] private Button prepSceneButton;

    [Header("Event Display")]
    [SerializeField] private Transform eventButtonContainer;
    [SerializeField] private GameObject eventButtonPrefab;

    [Header("Level Up Panel")]
    [SerializeField] private GameObject levelUpOverlay;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button continueButton;

    public static MapController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CheckLevelUp();
        UpdateUI();
        if (RunManager.Instance.currentDailyEvents.Count == 0 &&
            !RunManager.Instance.eventInProgress)
        {
            // First time - start with regular events
            RunManager.Instance.isBattleDay = false;
            RunManager.Instance.regularEventsCompleted = 0;
            RunManager.Instance.GenerateDailyEvents();
        }

        // Display current events
        DisplayEvents(RunManager.Instance.currentDailyEvents);

        prepSceneButton.onClick.AddListener(() =>
        {
            inspectTeam();
        });

    }



    public void UpdateUI()
    {
        if (RunManager.Instance != null)
        {
            goldText.text = $"Gold: {RunManager.Instance.currentGold}";
            dayText.text = $"Day: {RunManager.Instance.currentDay}";
            reputationText.text = $"Reputation: {RunManager.Instance.reputation} / 10";

            // Show event counter
            if (RunManager.Instance.isBattleDay)
            {
                dayTypeText.text = "BATTLE DAY";
                dayTypeText.color = Color.red;
                eventCounterText.text = "Choose your battle!";
            }
            else
            {
                dayTypeText.text = "EVENT DAY";
                dayTypeText.color = Color.blue;
                eventCounterText.text = $"Event {RunManager.Instance.regularEventsCompleted + 1}/3";
            }
        }
    }

    public void DisplayEvents(List<BaseEventSO> events)
    {
        foreach (Transform child in eventButtonContainer)
            Destroy(child.gameObject);

        // Create buttons for each event
        foreach (var eventSO in events)
        {
            GameObject buttonObj = Instantiate(eventButtonPrefab, eventButtonContainer);
            EventButtonUI buttonUI = buttonObj.GetComponent<EventButtonUI>();

            if (buttonUI != null)
            {
                buttonUI.Initialize(eventSO);
            }
        }

        // Update UI text
        UpdateUI();
    }

    void OnEnable()
    {

        // Always clear the event in progress flag when returning to map
        if (RunManager.Instance != null)
        {
            RunManager.Instance.eventInProgress = false;
            RunManager.Instance.selectedEvent = null;

            // Generate new events if we have none
            if (RunManager.Instance.currentDailyEvents.Count == 0)
            {
                RunManager.Instance.GenerateDailyEvents();
            }

            DisplayEvents(RunManager.Instance.currentDailyEvents);
        }

        UpdateUI();
    }


    public void inspectTeam()
    {
        SceneLoader.Instance.LoadScene(GameScene.PrepScene);
    }

    private void CheckLevelUp()
    {
        if (RunManager.Instance == null) return;
        if (RunManager.Instance.reputation >= 10)
        {
            RunManager.Instance.reputation -= 10;
            RunManager.Instance.playerLevel++;

            levelUpOverlay.SetActive(true);
            levelText.text = $"Level {RunManager.Instance.playerLevel}";
            rewardText.text = "+2 Provision";

            continueButton.onClick.AddListener(() =>
            {
                levelUpOverlay.SetActive(false);
                RunManager.Instance.provisionCap += 2;
            });
        }
    }
}
