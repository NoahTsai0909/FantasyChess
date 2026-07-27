using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PortalArtifactUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image eventIcon;
    [SerializeField] private Button invisibleSelectButton;

    [Header("Portal Visuals")]
    [SerializeField] private RectTransform artifactRoot;
    [SerializeField] private RectTransform shadowTransform;
    [SerializeField] private Image energyRing;

    [Header("Animation Settings")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 15f;

    private BaseEventSO currentEvent;
    private Vector3 originalArtifactPos;
    private Vector3 originalShadowScale;
    private bool isHovered = false;
    private Color baseColor;

    public void Initialize(BaseEventSO eventSO)
    {
        currentEvent = eventSO;
        eventIcon.sprite = eventSO.eventIcon;

        baseColor = eventSO.portalGlowColor;

        baseColor.a = 0.5f; //0.5 when unhovered
        energyRing.color = baseColor;

        originalArtifactPos = artifactRoot.anchoredPosition;
        originalShadowScale = shadowTransform.localScale;

        invisibleSelectButton.onClick.RemoveAllListeners();
        invisibleSelectButton.onClick.AddListener(OnEventSelected);
    }

    private void Update()
    {
        float sineWave = Mathf.Sin(Time.time * bobSpeed);
        artifactRoot.anchoredPosition = originalArtifactPos + new Vector3(0f, sineWave * bobHeight, 0f);

        float shadowScale = 1f - (sineWave * 0.2f);
        shadowTransform.localScale = originalShadowScale * shadowScale;

        float targetAlpha = isHovered ? 1f : 0.5f;
        baseColor.a = Mathf.Lerp(baseColor.a, targetAlpha, Time.deltaTime * 10f);
        energyRing.color = baseColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        // Pass this object's stable transform.position to the MapController
        MapController.Instance.ShowEventInfo(
            currentEvent.eventName,
            currentEvent.description,
            transform.position
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        // Tell the MapController to hide the HUD when the mouse leaves
        MapController.Instance.HideEventInfo();
    }

    private void OnEventSelected()
    {
        if (currentEvent != null)
        {
            // Ensure the HUD hides before transitioning scenes!
            MapController.Instance.HideEventInfo();
            currentEvent.OnSelected();
        }
    }

    public void SetElevation(float yOffset)
    {
        // Adjust the baseline position the sine wave bobs around
        originalArtifactPos.y += yOffset;

        // Move the shadow up so the "floor" is physically higher
        shadowTransform.anchoredPosition = new Vector2(
            shadowTransform.anchoredPosition.x,
            shadowTransform.anchoredPosition.y + yOffset
        );
    }
}