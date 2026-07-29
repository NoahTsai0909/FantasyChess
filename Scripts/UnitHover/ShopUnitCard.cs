using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class ShopUnitCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image unitPortrait;
    [SerializeField] private UnitHoverUI innerUnitUI;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI priceText;

    private UnitInstance myDummyUnit; // Keep track so we can destroy it later if needed

    public void Initialize(UnitInstance dummyUnit, int price, UnityAction onBuyClicked)
    {
        myDummyUnit = dummyUnit;

        // Set the Character Art
        if (dummyUnit.Definition.unitSprite != null)
        {
            unitPortrait.sprite = dummyUnit.Definition.unitSprite;
        }

        // Pass the live dummy unit to your ORIGINAL Show method!
        innerUnitUI.Show(dummyUnit);

        // Set up the Buy Button
        priceText.text = $"BUY ({price}G)";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(onBuyClicked);
    }

    public void MarkAsPurchased()
    {
        // A CanvasGroup allows us to make the entire card invisible and unclickable
        // while preserving its RectTransform so the Layout Group still sees it.
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;               // Make it 100% transparent
        cg.interactable = false;     // Disable all button clicks
        cg.blocksRaycasts = false;   // Stop it from blocking the mouse
    }
}
