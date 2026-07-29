using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems; // Required for Pointer interfaces

// Add the Pointer interfaces to detect hover and click natively
public class ShopUnitCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image unitPortrait;
    [SerializeField] private UnitHoverUI innerUnitUI;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Hover Glow Settings")]
    [SerializeField] private CanvasGroup hoverGlowGroup; // The backdrop image we will fade in/out
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 0.8f;

    private UnityAction onBuyClicked;
    private bool isHovered = false;

    public void Initialize(UnitInstance dummyUnit, int price, UnityAction onBuyClicked)
    {
        // 1. Portrait setup
        if (dummyUnit.Definition.unitSprite != null)
        {
            unitPortrait.sprite = dummyUnit.Definition.unitSprite;
        }

        // 2. Data setup
        innerUnitUI.Show(dummyUnit);
        priceText.text = $"{TextIconUtility.FormatGold(price)}";
        this.onBuyClicked = onBuyClicked;

        // 3. Ensure the glow is fully invisible when the card is first created
        if (hoverGlowGroup != null)
        {
            hoverGlowGroup.alpha = 0f;
        }
    }

    void Update()
    {
        // If hovered, calculate a sine wave to smoothly ping-pong the alpha value
        if (isHovered && hoverGlowGroup != null)
        {
            float sineWave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // Returns a value between 0 and 1
            hoverGlowGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, sineWave);
        }
    }

    // Triggered the moment the mouse enters the card's RectTransform
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        hoverGlowGroup.gameObject.SetActive(true); // Ensure the glow is active when hovered
    }

    // Triggered the moment the mouse leaves the card's RectTransform
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (hoverGlowGroup != null)
        {
            hoverGlowGroup.alpha = 0f; // Instantly hide the glow when the mouse leaves
        }
        hoverGlowGroup.gameObject.SetActive(false); // Deactivate the glow when not hovered
    }

    // Triggered when the user clicks anywhere on this card
    public void OnPointerClick(PointerEventData eventData)
    {
        onBuyClicked?.Invoke();
    }

    // The method we made earlier to hide the card without breaking the layout
    public void MarkAsPurchased()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
