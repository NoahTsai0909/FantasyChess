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

    [Header("Event Info HUD")]
    [SerializeField] private GameObject eventInfoPanel; // Assign your new HUD image here
    [SerializeField] private TextMeshProUGUI infoTitleText;
    [SerializeField] private TextMeshProUGUI infoDescText;

    [Header("Transition Overlay")]
    [SerializeField] private Image blackScreenOverlay;

    [Tooltip("How far from the portal should the HUD appear?")]
    [SerializeField] private Vector3 hoverOffset = new Vector3(0, 100f, 0);

    public static MapController Instance { get; private set; }

    public bool isTransitioning = false;

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

        // Use a traditional for-loop so we know the index number (0, 1, or 2)
        for (int i = 0; i < events.Count; i++)
        {
            GameObject buttonObj = Instantiate(eventButtonPrefab, eventButtonContainer);
            PortalArtifactUI portalUI = buttonObj.GetComponent<PortalArtifactUI>();

            if (portalUI != null)
            {
                portalUI.Initialize(events[i]);

                // If there are exactly 3 events, and this is the middle one (index 1)
                if (events.Count == 3 && i == 1)
                {
                    // Push it up by 60 pixels (Adjust this number to your liking!)
                    portalUI.SetElevation(60f);
                }
            }
        }

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
        isTransitioning = false;
        UpdateUI();
    }


    public void inspectTeam()
    {
        SceneLoader.Instance.LoadScene(GameScene.PrepScene);
    }

    private void CheckLevelUp()
    {
        if (RunManager.Instance == null) return;
        if (RunManager.Instance.Stats.Experience >= 10)
        {
            RunManager.Instance.Stats.Experience -= 10;
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

    public void ShowEventInfo(string eventName, string eventDescription, Vector3 targetPosition)
    {
        infoTitleText.text = eventName;
        infoDescText.text = eventDescription;

        Canvas canvas = eventInfoPanel.GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1f;

        // Apply the scaled offset
        eventInfoPanel.transform.position = targetPosition + (hoverOffset * scale);

        eventInfoPanel.SetActive(true);
    }

    public void HideEventInfo()
    {
        eventInfoPanel.SetActive(false);
    }

    public void SetOverlayAlpha(float alpha)
    {
        if (blackScreenOverlay != null)
        {
            // Turn it on the moment we need it
            if (!blackScreenOverlay.gameObject.activeSelf && alpha > 0f)
            {
                blackScreenOverlay.gameObject.SetActive(true);
            }

            blackScreenOverlay.color = new Color(0, 0, 0, alpha);
        }
    }

}
