using UnityEngine;
using TMPro; // Needed for text
using DG.Tweening; // Needed for DOTween

public class NotificationManager : MonoBehaviour
{
    // --- The Singleton Instance ---
    public static NotificationManager Instance;

    [Header("UI References")]
    public RectTransform bannerRect;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI notificationText; // Drag your Text object here!

    [Header("Settings")]
    public float holdTime = 2.5f;

    private Sequence activeSequence; // Tracks the current animation

    private void Awake()
    {
        // Set up the Singleton so any script can find this!
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Ensure it starts invisible
        bannerRect.localScale = new Vector3(0f, 0.05f, 1f);
        canvasGroup.alpha = 0f;
    }

    // Notice we added a 'string message' parameter here!
    public void ShowNotification(string message)
    {
        // 1. Safety check: If an animation is already playing, stop it instantly!
        if (activeSequence != null && activeSequence.IsActive())
        {
            activeSequence.Kill();
        }

        // 2. Set the text
        notificationText.text = message;

        // 3. Reset to starting position
        bannerRect.localScale = new Vector3(0f, 0.05f, 1f);
        canvasGroup.alpha = 0f;

        // 4. Build and play the sequence
        activeSequence = DOTween.Sequence();

        // The Strike
        activeSequence.Append(bannerRect.DOScaleX(1f, 0.15f).SetEase(Ease.OutQuad));

        // The Expand & Reveal
        activeSequence.Append(bannerRect.DOScaleY(1f, 0.2f).SetEase(Ease.OutBack));
        activeSequence.Join(canvasGroup.DOFade(1f, 0.2f));

        // The Hold
        activeSequence.AppendInterval(holdTime);

        // The Collapse
        activeSequence.Append(bannerRect.DOScaleY(0.05f, 0.2f).SetEase(Ease.InBack));
        activeSequence.Join(canvasGroup.DOFade(0f, 0.2f));

        // The Vanish
        activeSequence.Append(bannerRect.DOScaleX(0f, 0.15f).SetEase(Ease.InQuad));
    }
}
