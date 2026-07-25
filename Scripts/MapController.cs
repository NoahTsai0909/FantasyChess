using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneLoader;
using TMPro;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
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
            RunManager.Instance.isBattlePhase = false;
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
        if (RunManager.Instance.Stats.Reputation >= 10)
        {
            RunManager.Instance.Stats.Reputation -= 10;
            RunManager.Instance.Stats.PlayerLevel++;

            levelUpOverlay.SetActive(true);
            levelText.text = $"Level {RunManager.Instance.Stats.PlayerLevel}";
            rewardText.text = "+2 Provision";

            continueButton.onClick.AddListener(() =>
            {
                levelUpOverlay.SetActive(false);
                RunManager.Instance.Stats.ProvisionCap += 2;
            });
        }
    }
}
