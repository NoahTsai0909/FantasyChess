using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneLoader;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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

    [Header("Encounter Preview")]
    [SerializeField] private GameObject previewOverlay; // The dark UI panel
    [SerializeField] private Button closePreviewButton;
    [SerializeField] private GridManager previewGrid;

    [Header("Transition Overlay")]
    [SerializeField] private Image blackScreenOverlay;

    [Tooltip("How far from the portal should the HUD appear?")]
    [SerializeField] private Vector3 hoverOffset = new Vector3(0, 100f, 0);

    public static MapController Instance { get; private set; }

    public bool isTransitioning = false;
    private bool isPinned = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // If we leveled up, CheckLevelUp() triggers a scene load. 
        // We return instantly so we don't waste power generating the map.
        if (CheckLevelUp()) return;

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

        if (closePreviewButton != null)
        {
            closePreviewButton.onClick.AddListener(ClosePreview);
        }

        if (previewOverlay != null) previewOverlay.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        // 1. If currently pinned, listen for unpin triggers
        if (isPinned)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame ||
                Keyboard.current.escapeKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame)
            {
                isPinned = false;

                // Turn raycasts back off so it doesn't cause glitches when it unpins!
                CanvasGroup cg = eventInfoPanel.GetComponent<CanvasGroup>();
                if (cg != null) cg.blocksRaycasts = false;

                HideEventInfo(); // Force it to hide once unpinned
                if (TooltipUIManager.Instance != null) TooltipUIManager.Instance.Hide();
            }
            return;
        }

        // 2. Just regular pinning for the main panel!
        if (eventInfoPanel.activeSelf && Keyboard.current.tKey.wasPressedThisFrame)
        {
            isPinned = true;

            // NEW: The moment it is pinned, make it solid so the player can hover the text links!
            CanvasGroup cg = eventInfoPanel.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = true;
        }
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
        isPinned = false;
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

    private bool CheckLevelUp()
    {
        if (RunManager.Instance == null || RunManager.Instance.currentRegionTree == null) return false;

        int targetIndex = RunManager.Instance.Stats.PlayerLevel - 1;
        int safeIndex = Mathf.Clamp(targetIndex, 0, RunManager.Instance.currentRegionTree.levelNodes.Count - 1);

        if (RunManager.Instance.currentRegionTree.levelNodes.Count == 0) return false;

        LevelUpEventSO nextLevelEvent = RunManager.Instance.currentRegionTree.levelNodes[safeIndex];

        if (RunManager.Instance.Stats.Experience >= nextLevelEvent.xpRequired)
        {
            RunManager.Instance.Stats.Experience -= nextLevelEvent.xpRequired;
            RunManager.Instance.Stats.PlayerLevel++;

            nextLevelEvent.OnSelected();
            return true; 
        }

        return false;
    }

    public void ShowEventInfo(string eventName, string eventDescription, Vector3 targetPosition)
    {
        if (isPinned) return; // Ignore new hover attempts if one is already pinned

        infoTitleText.text = eventName;
        infoDescText.SetText(TextIconUtility.ParseDescription(eventDescription));

        Canvas canvas = eventInfoPanel.GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1f;

        // Apply the scaled offset
        eventInfoPanel.transform.position = targetPosition + (hoverOffset * scale);

        // NEW: Ensure it behaves like a ghost while just normally hovering
        CanvasGroup cg = eventInfoPanel.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = false;

        eventInfoPanel.SetActive(true);
    }

    public void HideEventInfo()
    {
        if (isPinned) return; // Refuse to close if the player pinned it

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

    public void PreviewEncounter(EncounterDefinition encounter)
    {
        if (encounter == null) return;

        // 1. Show the dark overlay to hide the map
        if (previewOverlay != null) previewOverlay.SetActive(true);

        // NEW: Hide the UI portals so they don't block the world-space grid!
        if (eventButtonContainer != null) eventButtonContainer.gameObject.SetActive(false);
        if (closePreviewButton != null) closePreviewButton.gameObject.SetActive(true);
        if (previewGrid != null)
        {
            previewGrid.gameObject.SetActive(true);
        }
        // 2. Ensure the grid is clean
        previewGrid.ClearAllUnits();

        // 3. Spawn the enemies just like the GameManager does
        foreach (var placement in encounter.enemyUnits)
        {
            if (placement.unitData == null || placement.unitData.definition == null) continue;

            UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);
            unit.InitializeEnemy(placement.unitData.definition, placement.unitData.rarity);

            unit.EnterCombat(previewGrid, placement.row, placement.col, false, false);
        }
    }

    public void ClosePreview()
    {
        // Wipe the dummy units and hide the overlay
        previewGrid.ClearAllUnits();
        previewGrid.gameObject.SetActive(false);
        if (closePreviewButton != null) closePreviewButton.gameObject.SetActive(false);
        if (previewOverlay != null) previewOverlay.SetActive(false);

        // NEW: Bring the portals back!
        if (eventButtonContainer != null) eventButtonContainer.gameObject.SetActive(true);
    }

}
