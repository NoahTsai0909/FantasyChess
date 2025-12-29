using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SceneLoader;

public class EventSceneController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    //[SerializeField] private Image eventIcon;
    [SerializeField] private Button continueButton;
    [SerializeField] private Transform rewardAnchor;
    [SerializeField] private Button takeUnitButton;

    private UnitInstance previewUnit;
    private Rarity previewUnitRarity;

    void Start()
    {
        if (RunManager.Instance.selectedEvent != null)
        {
            var eventSO = RunManager.Instance.selectedEvent;

            // Display event info
            eventNameText.text = eventSO.eventName;
            descriptionText.text = eventSO.description;
            //eventIcon.sprite = eventSO.eventIcon;

            if (eventSO is RandomUnitEventSO)
            {
                //UnitDefinition randomUnit = eventSO.ReturnRandomUnit();
                UnitSaveData randomUnit = UnitGenerationService.GenerateUnit();
                ShowUnitPreview(randomUnit);

                takeUnitButton.gameObject.SetActive(true);
                takeUnitButton.onClick.AddListener(() =>
                {
                    //RunManager.Instance.AddUnitToBench(randomUnit);
                    if (PlayerUnitManager.Instance.TryAcquireUnit(randomUnit.definition, previewUnitRarity))
                    {
                        takeUnitButton.interactable = false;
                        CompleteEventAndReturn(eventSO);
                    }
                    else
                    {
                        Debug.Log("Error encountered when acquiring unit");
                    }
                });
            }
            // Continue button returns to map
            continueButton.onClick.AddListener(() =>
            {
                CompleteEventAndReturn(eventSO);
            });
        }
        else
        {
            Debug.LogError("No selected event found in EventScene!");
            // Fallback to map scene
            SceneLoader.Instance.LoadScene(GameScene.MapScene);
        }
    }

    private void ShowUnitPreview(UnitSaveData unit)
    {
        previewUnit = Instantiate(unit.definition.unitPrefab, rewardAnchor);
        previewUnit.InitializeFromSaveData(unit);
        previewUnitRarity = unit.rarity;
        previewUnit.enabled = false; // disables combat logic
        previewUnit.isPlayer = true;
        previewUnit.transform.localPosition = Vector3.zero;
    }

    private void CompleteEventAndReturn(BaseEventSO eventSO)
    {
        // Mark event as completed
        eventSO.CompleteEvent();

        // Return to map
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}
