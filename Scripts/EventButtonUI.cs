using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SceneLoader;

public class EventButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image eventIcon;
    [SerializeField] private Button selectButton;

    private BaseEventSO currentEvent;

    public void Initialize(BaseEventSO eventSO)
    {
        currentEvent = eventSO;

        eventNameText.text = eventSO.eventName;
        descriptionText.text = eventSO.description;
        eventIcon.sprite = eventSO.eventIcon;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnEventSelected);
    }

    private void OnEventSelected()
    {
        if (currentEvent != null)
        {
            Debug.Log($"Selected: {currentEvent.eventName}");

            currentEvent.OnSelected();
        }
    }
}
