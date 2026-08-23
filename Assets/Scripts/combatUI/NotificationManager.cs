using UnityEngine;
using TMPro;
using DG.Tweening;

public class NotificationManager : MonoBehaviour
{
    // --- The Singleton Instance ---
    public static NotificationManager Instance;

    [Header("UI References")]
    public RectTransform bannerRect;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI notificationText;

    [Header("Settings")]
    public float holdTime = 2.5f;

    private Sequence activeSequence; // Tracks the current animation

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Ensure it starts invisible
        bannerRect.localScale = new Vector3(0f, 0.05f, 1f);
        canvasGroup.alpha = 0f;
    }

    public void ShowNotification(string message)
    {
        if (activeSequence != null && activeSequence.IsActive())
        {
            activeSequence.Kill();
        }

        notificationText.text = message;

        bannerRect.localScale = new Vector3(0f, 0.05f, 1f);
        canvasGroup.alpha = 0f;

        activeSequence = DOTween.Sequence();

        activeSequence.Append(bannerRect.DOScaleX(1f, 0.15f).SetEase(Ease.OutQuad));

        activeSequence.Append(bannerRect.DOScaleY(1f, 0.2f).SetEase(Ease.OutBack));
        activeSequence.Join(canvasGroup.DOFade(1f, 0.2f));

        activeSequence.AppendInterval(holdTime);

        activeSequence.Append(bannerRect.DOScaleY(0.05f, 0.2f).SetEase(Ease.InBack));
        activeSequence.Join(canvasGroup.DOFade(0f, 0.2f));

        activeSequence.Append(bannerRect.DOScaleX(0f, 0.15f).SetEase(Ease.InQuad));
    }
}
