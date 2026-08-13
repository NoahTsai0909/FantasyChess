using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ShopUnitCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [Tooltip("The native UI Image that will display the unit's sprite")]
    [SerializeField] private Image unitPortrait;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Hover Glow Settings")]
    [SerializeField] private CanvasGroup hoverGlowGroup;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 0.8f;

    private UnitInstance assignedUnit;
    private UnityAction onBuyClicked;
    private bool isHovered = false;

    public void Initialize(UnitInstance dummyUnit, int price, UnityAction onBuyClicked)
    {
        assignedUnit = dummyUnit;

        // Restore the breathing and rarity outline animations
        if (assignedUnit != null && unitPortrait != null)
        {
            UIUnitVisualController visualController = unitPortrait.GetComponent<UIUnitVisualController>();
            if (visualController != null)
            {
                visualController.InitializeVisuals(assignedUnit.Definition, assignedUnit.CurrentRarity);
            }
        }

        priceText.text = $"{TextIconUtility.FormatGold(price)}";
        this.onBuyClicked = onBuyClicked;

        if (hoverGlowGroup != null)
        {
            hoverGlowGroup.alpha = 0f;
        }
    }

    void Update()
    {
        if (isHovered && hoverGlowGroup != null)
        {
            float sineWave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            hoverGlowGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, sineWave);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        hoverGlowGroup.gameObject.SetActive(true);

        if (UnitHoverDetector.Instance != null && assignedUnit != null)
        {
            // Pass this card's RectTransform as the anchor!
            UnitHoverDetector.Instance.ShowTooltipFromUI(assignedUnit);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        hoverGlowGroup.alpha = 0f;
        hoverGlowGroup.gameObject.SetActive(false);

        // HIDE THE TOOLTIP!
        if (UnitHoverDetector.Instance != null)
        {
            UnitHoverDetector.Instance.HideTooltipFromUI();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onBuyClicked?.Invoke();
    }

    public void MarkAsPurchased()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}