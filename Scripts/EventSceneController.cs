using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SceneLoader;

public class EventSceneController : MonoBehaviour
{
    [Header("Basic UI")]
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button continueButton;

    [Header("Dynamic Content")]
    [SerializeField] private Transform contentParent; // Where spawned UI will go

    [Header("Prefabs")]
    [SerializeField] private GameObject rewardEventUIPrefab;

    private BaseEventSO currentEvent;
    private GameObject spawnedUI;

    void Start()
    {
        currentEvent = RunManager.Instance.selectedEvent;

        if (currentEvent == null)
        {
            Debug.LogError("No selected event found!");
            SceneLoader.Instance.LoadScene(GameScene.MapScene);
            return;
        }

        // Display basic event info
        eventNameText.text = currentEvent.eventName;
        descriptionText.text = currentEvent.description;

        // Load appropriate UI based on event type
        LoadEventUI();
    }

    private void LoadEventUI()
    {
        // For now, we only have reward events (unit and gold)
        // Later you can add more types
        if (currentEvent is RandomUnitEventSO || currentEvent is GoldEventSO)
        {
            spawnedUI = Instantiate(rewardEventUIPrefab, contentParent);
            spawnedUI.GetComponent<RewardEventUI>().Setup(currentEvent, this);
        }
        else
        {
            Debug.LogWarning($"No UI prefab defined for event type: {currentEvent.GetType()}");
        }

        continueButton.onClick.AddListener(CompleteEvent);
        
    }


    public void CompleteEvent()
    {
        // Clean up spawned UI
        if (spawnedUI != null)
            Destroy(spawnedUI);

        // Complete the event and return to map
        currentEvent.CompleteEvent();
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }


    /*private void ShowUnitPreview(UnitSaveData unit)
    {
        previewUnit = Instantiate(unit.definition.unitPrefab, rewardAnchor);
        previewUnit.InitializeFromSaveData(unit);
        previewUnitRarity = unit.rarity;
        previewUnit.enabled = false; // disables combat logic
        previewUnit.isPlayer = true;
        previewUnit.transform.localPosition = Vector3.zero;
    }*/

}
