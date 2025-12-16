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
                UnitDefinition randomUnit = eventSO.ReturnRandomUnit();
                ShowUnitPreview(randomUnit);

                takeUnitButton.gameObject.SetActive(true);
                takeUnitButton.onClick.AddListener(() =>
                {
                    RunManager.Instance.AddUnitToBench(randomUnit);
                    Debug.Log($"Recruited unit: {randomUnit.unitName}");
                    takeUnitButton.interactable = false;
                    CompleteEventAndReturn(eventSO);
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

    private void ShowUnitPreview(UnitDefinition unitDef)
    {
        previewUnit = Instantiate(unitDef.unitPrefab, rewardAnchor);

        previewUnit.enabled = false; // disables combat logic
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
