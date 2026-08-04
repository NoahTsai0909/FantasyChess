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
    [SerializeField] private Image outerGlow;

    [Header("Animation Settings")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 15f;

    [Header("Glow Settings")]
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float idleGlowMin = 0.3f;
    [SerializeField] private float idleGlowMax = 0.6f;

    private BaseEventSO currentEvent;
    private Vector3 originalArtifactPos;
    private Vector3 originalShadowScale;
    private bool isHovered = false;
    private Color baseColor;
    private bool isTransitioning = false;

    public void Initialize(BaseEventSO eventSO)
    {
        currentEvent = eventSO;
        eventIcon.sprite = eventSO.eventIcon;
        baseColor = eventSO.portalGlowColor;

        originalArtifactPos = artifactRoot.anchoredPosition;
        originalShadowScale = shadowTransform.localScale;

        invisibleSelectButton.onClick.RemoveAllListeners();
        invisibleSelectButton.onClick.AddListener(OnEventSelected);
    }

    public void SetElevation(float yOffset)
    {
        originalArtifactPos.y += yOffset;
        shadowTransform.anchoredPosition = new Vector2(
            shadowTransform.anchoredPosition.x,
            shadowTransform.anchoredPosition.y + yOffset
        );
    }

    private void Update()
    {
        if (isTransitioning) return;
        // 1. Hover Bobbing
        float sineWave = Mathf.Sin(Time.time * bobSpeed);
        artifactRoot.anchoredPosition = originalArtifactPos + new Vector3(0f, sineWave * bobHeight, 0f);

        float shadowScale = 1f - (sineWave * 0.2f);
        shadowTransform.localScale = originalShadowScale * shadowScale;

        // 2. Continuous Breathing Glow
        // Maps a sine wave from -1 to 1 into a smooth 0 to 1 range
        float breathingMath = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        // If hovered, blast it to 1.0 (100%). If idle, pulse smoothly between Min and Max.
        float targetGlow = isHovered ? 1f : Mathf.Lerp(idleGlowMin, idleGlowMax, breathingMath);

        Color ringColor = baseColor;
        ringColor.a = Mathf.Lerp(energyRing.color.a, targetGlow, Time.deltaTime * 10f);

        // Apply the color to both rings, but make the blurry outer ring slightly softer
        energyRing.color = ringColor;

        if (outerGlow != null)
        {
            Color outerColor = ringColor;
            outerColor.a *= 0.6f; // The blur is 60% as intense as the sharp core
            outerGlow.color = outerColor;

            // Bonus: Slightly expand the blurry light when hovered!
            float scaleTarget = isHovered ? 1.15f : 1.0f;
            outerGlow.rectTransform.localScale = Vector3.Lerp(outerGlow.rectTransform.localScale, Vector3.one * scaleTarget, Time.deltaTime * 10f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (MapController.Instance == null) return;
        if (currentEvent == null) return;
        MapController.Instance.ShowEventInfo(currentEvent.eventName, currentEvent.description, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        MapController.Instance.HideEventInfo();
    }

    private void OnEventSelected()
    {
        if (currentEvent != null && !MapController.Instance.isTransitioning)
        {
            // Lock out all other portals on the map
            MapController.Instance.isTransitioning = true;

            // Lock this specific portal's Update loop
            isTransitioning = true;

            // Hide the UI text and disable this button
            MapController.Instance.HideEventInfo();
            invisibleSelectButton.interactable = false;

            // Start the cinematic zoom!
            StartCoroutine(ZoomIntoVoidTransition());
        }
    }

    private System.Collections.IEnumerator ZoomIntoVoidTransition()
    {
        transform.SetAsLastSibling();

        float duration = 0.6f;
        float elapsed = 0f;

        Vector3 startPos = artifactRoot.position;
        Vector3 targetPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        Vector3 startScale = artifactRoot.localScale;
        Vector3 targetScale = Vector3.one * 50f;

        // Reset the overlay to 0% via the MapController
        MapController.Instance.SetOverlayAlpha(0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            artifactRoot.position = Vector3.Lerp(startPos, targetPos, t);
            artifactRoot.localScale = Vector3.Lerp(startScale, targetScale, t);

            // Tell the MapController to fade the screen!
            MapController.Instance.SetOverlayAlpha(t);

            yield return null;
        }

        // The screen is now 100% pitch black. Execute the load!
        currentEvent.OnSelected();
    }
}