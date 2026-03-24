// EventSceneController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;

public class EventSceneController : MonoBehaviour
{
    [Header("Basic UI")]
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button continueButton;
    [SerializeField] private SpriteRenderer eventBackgroundRenderer;

    [Header("Dynamic Content")]
    [SerializeField] private Transform contentParent; // For UI prefabs
    [SerializeField] private Transform unitPreviewAnchor; // The world space anchor!
    [SerializeField] private GameObject rewardEventUIPrefab;

    private BaseEventSO currentEvent;
    private GameObject spawnedUI;
    private UnitSaveData currentUnitPreviewSaveData;
    private UnitInstance currentUnitPreview;

    void Start()
    {
        currentEvent = RunManager.Instance.selectedEvent;

        if (currentEvent == null)
        {
            SceneLoader.Instance.LoadScene(GameScene.MapScene);
            return;
        }

        // Display basic event info
        eventNameText.text = currentEvent.eventName;
        descriptionText.text = currentEvent.description;

        if (eventBackgroundRenderer != null && currentEvent.eventBackgroundImage != null)
            eventBackgroundRenderer.sprite = currentEvent.eventBackgroundImage;

        LoadEventUI();
    }

    private void LoadEventUI()
    {
        // Spawn the UI prefab
        spawnedUI = Instantiate(rewardEventUIPrefab, contentParent);
        var rewardUI = spawnedUI.GetComponent<RewardEventUI>();
        rewardUI.Setup(currentEvent, this);

        // Handle unit preview - use the world space anchor!
        if (currentEvent is RandomUnitEventSO unitEvent)
        {
            currentUnitPreviewSaveData = unitEvent.ReturnRandomUnit();
            ShowUnitPreview(currentUnitPreviewSaveData);
        }
        else if (currentEvent is PresetUnitEventSO presetUnitEvent)
        {
            currentUnitPreviewSaveData = presetUnitEvent.ReturnRandomUnit();
            ShowUnitPreview(currentUnitPreviewSaveData);
        }

            continueButton.onClick.AddListener(CompleteEvent);
    }

    private void ShowUnitPreview(UnitSaveData unit)
    {
        // Use the world space anchor, NOT the one inside UI
        currentUnitPreview = Instantiate(unit.definition.unitPrefab, unitPreviewAnchor);
        currentUnitPreview.InitializeFromSaveData(unit);
        currentUnitPreview.isPlayer = true;
        currentUnitPreview.enabled = false;
        currentUnitPreview.transform.localPosition = Vector3.zero;
        currentUnitPreview.transform.localScale = Vector3.one * 1.25f;

        // Force it to render on top
        SpriteRenderer renderer = currentUnitPreview.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 100;
        }
    }

    public void CompleteEvent()
    {
        if (currentUnitPreview != null)
            Destroy(currentUnitPreview.gameObject);

        if (spawnedUI != null)
            Destroy(spawnedUI);

        currentEvent.CompleteEvent();
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }

    public void ObtainCurrentUnit()
    {
        PlayerUnitManager.Instance.TryAcquireUnit(currentUnitPreviewSaveData.definition, currentUnitPreviewSaveData.rarity);
        
    }
}