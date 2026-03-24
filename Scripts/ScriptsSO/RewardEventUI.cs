// RewardEventUI.cs - Now doesn't need rewardAnchor!
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardEventUI : MonoBehaviour
{
    [Header("UI References")]
    public Button takeRewardButton;
    public TMP_Text takeRewardButtonText;

    private BaseEventSO currentEvent;
    private EventSceneController sceneController;

    public void Setup(BaseEventSO eventData, EventSceneController controller)
    {
        currentEvent = eventData;
        sceneController = controller;

        if (eventData is RandomUnitEventSO unitEvent)
        {
            takeRewardButtonText.text = unitEvent.eventButtonText ?? "RECRUIT";
            takeRewardButton.onClick.AddListener(() => {
                controller.ObtainCurrentUnit();
                RewardTaken();
            });
        }
        else if (eventData is GoldEventSO goldEvent)
        {
            int goldAmount = goldEvent.getGoldAmount();
            takeRewardButtonText.text = goldEvent.eventButtonText ?? $"TAKE {goldAmount} GOLD";
            takeRewardButton.onClick.AddListener(() => {
                RunManager.Instance.currentGold += goldAmount;
                RewardTaken();
            });
        }
        else if (eventData is PresetUnitEventSO presetEvent)
        {
            if (presetEvent.isUnit)
            {
                takeRewardButtonText.text = "RECRUIT";
            }
            else
            {
                takeRewardButtonText.text = "OBTAIN";
            }
            takeRewardButton.onClick.AddListener(() => {
                controller.ObtainCurrentUnit();
                RewardTaken();
            });
        }
    }

    private void RewardTaken()
    {
        takeRewardButton.gameObject.SetActive(false);
        sceneController.CompleteEvent();
    }
}