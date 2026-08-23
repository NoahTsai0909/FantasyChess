using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UniversalPopupManager : MonoBehaviour
{
    public static UniversalPopupManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private TextMeshProUGUI popupText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float defaultDisplayDuration = 2.5f;

    private Coroutine activePopupCoroutine;

    private void Awake()
    {
        // Enforce the Singleton pattern and keep it alive across all scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure it starts completely invisible
            if (popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = 0f;
                popupCanvasGroup.gameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this from ANY script using: UniversalPopupManager.ShowPopup("Message");
    /// </summary>
    public static void ShowPopup(string message, float duration = -1f)
    {
        if (Instance != null)
        {
            float time = duration > 0 ? duration : Instance.defaultDisplayDuration;
            Instance.DisplayPopup(message, time);
        }
        else
        {
            Debug.LogWarning($"UniversalPopupManager is missing! Could not show message: {message}");
        }
    }

    private void DisplayPopup(string message, float duration)
    {
        // If a popup is already on screen, interrupt its fade and instantly swap the text
        if (activePopupCoroutine != null)
        {
            StopCoroutine(activePopupCoroutine);
        }

        activePopupCoroutine = StartCoroutine(PopupRoutine(message, duration));
    }

    private IEnumerator PopupRoutine(string message, float duration)
    {
        popupText.SetText(TextIconUtility.ParseDescription(message));
        popupCanvasGroup.gameObject.SetActive(true);
        popupCanvasGroup.transform.localScale = Vector3.one * 0.9f;

        // 1. Fade In and slightly scale up (gives it a nice "pop")
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            popupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            popupCanvasGroup.transform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
            yield return null;
        }
        popupCanvasGroup.alpha = 1f;

        // 2. Linger on screen
        yield return new WaitForSeconds(duration);

        // 3. Fade Out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            popupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.gameObject.SetActive(false);
    }
}
