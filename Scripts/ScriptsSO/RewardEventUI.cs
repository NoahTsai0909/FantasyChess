// RewardEventUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardEventUI : MonoBehaviour
{
    [Header("UI References")]
    public Button takeRewardButton;
    public TMP_Text takeRewardButtonText;
    public Transform rewardAnchor;
    public Image backgroundImage;
    private BaseEventSO currentEvent;
    private EventSceneController sceneController;

    public void Setup(BaseEventSO eventData, EventSceneController controller)
    {
        currentEvent = eventData;
        sceneController = controller;

        if (backgroundImage != null && eventData.eventBackgroundImage != null)
        {
            backgroundImage.sprite = eventData.eventBackgroundImage;
        }
        // Handle different event types
        if (eventData is RandomUnitEventSO unitEvent)
        {
            SetupForUnit(unitEvent);
        }
        else if (eventData is GoldEventSO goldEvent)
        {
            SetupForGold(goldEvent);
        }

    }

    private void SetupForUnit(RandomUnitEventSO unitEvent)
    {
        UnitSaveData unit = unitEvent.ReturnRandomUnit();
        UnitInstance previewUnit = Instantiate(unit.definition.unitPrefab, rewardAnchor);
        previewUnit.InitializeFromSaveData(unit);
        previewUnit.isPlayer = true;

        // Fix scale and layering
        previewUnit.transform.localScale = Vector3.one * 75f;
        previewUnit.transform.localPosition = Vector3.zero;

        // Fix sorting layer for the unit
        SpriteRenderer renderer = previewUnit.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 100; // High number to render above UI
        }

        takeRewardButtonText.text = unitEvent.eventButtonText ?? "RECRUIT";

        takeRewardButton.onClick.AddListener(() =>
        {
            if (PlayerUnitManager.Instance.TryAcquireUnit(unit.definition, unit.rarity))
            {
                RewardTaken();
            }
        });
    }


    private void SetupForGold(GoldEventSO goldEvent)
    {
        int goldAmount = goldEvent.getGoldAmount();

        // Display gold info
        takeRewardButtonText.text = goldEvent.eventButtonText ?? $"TAKE {goldEvent.goldAmount} GOLD";

        // Setup button action
        takeRewardButton.onClick.AddListener(() => {
            RunManager.Instance.currentGold += goldAmount;
            Debug.Log($"Gained {goldAmount} gold");
            RewardTaken();
        });
    }

    private void RewardTaken()
    {
        // Disable take button, show continue button
        takeRewardButton.gameObject.SetActive(false);
    }
}
