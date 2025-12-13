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
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private GameObject rewardPrefab;

    void Start()
    {
        if (RunManager.Instance.selectedEvent != null)
        {
            var eventSO = RunManager.Instance.selectedEvent;

            // Display event info
            eventNameText.text = eventSO.eventName;
            descriptionText.text = eventSO.description;
            //eventIcon.sprite = eventSO.eventIcon;

            // Apply rewards immediately
            ApplyEventRewards(eventSO);

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

    private void ApplyEventRewards(BaseEventSO eventSO)
    {
        // Apply the event's rewards
        if (eventSO.possibleRewards != null && eventSO.possibleRewards.Length > 0)
        {
            foreach (var reward in eventSO.possibleRewards)
            {
                reward.Apply();

                // Display reward visually
                if (rewardPrefab != null && rewardContainer != null)
                {
                    GameObject rewardObj = Instantiate(rewardPrefab, rewardContainer);
                    // Setup reward display...
                }
            }
        }
    }

    private void CompleteEventAndReturn(BaseEventSO eventSO)
    {
        // Mark event as completed
        eventSO.CompleteEvent();

        // Return to map
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}
