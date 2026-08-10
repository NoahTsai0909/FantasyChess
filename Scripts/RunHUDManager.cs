using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using static SceneLoader;

public class RunHUDManager : MonoBehaviour
{
    public static RunHUDManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private TMP_Text playerXPText;
    [SerializeField] private Image playerXPFill;
    [SerializeField] private TMP_Text provisionCapText;
    [SerializeField] private TMP_Text goldText;

    [Header("XP Settings")]
    [SerializeField] private int maxReputation = 10; // Max reputation for level up

    [Header("Animation Settings")]
    private RectTransform hudRect;
    private Vector2 originalAnchoredPos;

    [Header("References")]
    [SerializeField] private GameObject runHUD;
    [SerializeField] private CanvasGroup runHUDCanvasGroup; // optional fallback

    [Header("Behavior")]
    [SerializeField] private bool dontDestroyOnLoad = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (runHUD != null)
        {
            hudRect = runHUD.GetComponent<RectTransform>();
            if (hudRect != null)
            {
                originalAnchoredPos = hudRect.anchoredPosition;
            }
        }
        StartCoroutine(InitializeFromRunManager());
    }

    private void OnEnable()
    {
        // Subscribe to all events
        RunStatsEventBus.OnGoldChanged += UpdateGold;
        RunStatsEventBus.OnHealthChanged += UpdateHealth;
        RunStatsEventBus.OnDayChanged += UpdateDay;
        RunStatsEventBus.OnLevelChanged += UpdateLevel;
        RunStatsEventBus.OnReputationChanged += UpdateReputation;
        RunStatsEventBus.OnProvisionCapChanged += UpdateProvisionCap;
    }

    private void OnDisable()
    {
        // Unsubscribe from all events
        RunStatsEventBus.OnGoldChanged -= UpdateGold;
        RunStatsEventBus.OnHealthChanged -= UpdateHealth;
        RunStatsEventBus.OnDayChanged -= UpdateDay;
        RunStatsEventBus.OnLevelChanged -= UpdateLevel;
        RunStatsEventBus.OnReputationChanged -= UpdateReputation;
        RunStatsEventBus.OnProvisionCapChanged -= UpdateProvisionCap;
    }

    private IEnumerator InitializeFromRunManager()
    {
        // Wait a frame to ensure RunManager is fully initialized
        yield return null;

        if (RunManager.Instance != null)
        {
            UpdateGold(RunManager.Instance.Stats.CurrentGold);
            UpdateHealth(RunManager.Instance.Stats.PlayerHealth);
            UpdateDay(RunManager.Instance.Stats.CurrentDay);
            UpdateLevel(RunManager.Instance.Stats.PlayerLevel);
            UpdateReputation(RunManager.Instance.Stats.Experience);
            UpdateProvisionCap(RunManager.Instance.Stats.ProvisionCap);
        }
    }

    // UI Update Methods
    private void UpdateGold(int gold)
    {
        if (goldText != null)
            goldText.SetText(TextIconUtility.ParseDescription("[c_gold]" + gold.ToString() + "[/c]")); 
    }

    private void UpdateHealth(int health)
    {
        if (playerHealthText != null)
            playerHealthText.SetText(TextIconUtility.ParseDescription("[c_playerhealth]" + health.ToString() + "[/c]"));
    }

    private void UpdateDay(int day)
    {
        if (dayText != null)
            dayText.SetText(TextIconUtility.ParseDescription("Day \n" + "[c_day]" + day.ToString() + "[/c]"));
    }

    private void UpdateLevel(int level)
    {
        if (playerXPText != null)
            playerXPText.SetText(TextIconUtility.ParseDescription("[c_level]" + level.ToString() + "[/c]"));
    }

    private void UpdateReputation(int reputation)
    {
        if (playerXPFill != null)
        {
            // Calculate fill amount: reputation / maxReputation
            float fillAmount = (float)reputation / maxReputation;
            playerXPFill.fillAmount = Mathf.Clamp01(fillAmount);

            // Optional: Add color gradient based on fill amount
            // playerXPFill.color = Color.Lerp(Color.red, Color.green, fillAmount);
        }
    }

    private void UpdateProvisionCap(int cap)
    {
        if (provisionCapText != null)
            provisionCapText.SetText(TextIconUtility.ParseDescription("[c_maxprovision]" + cap.ToString() + "[/c]"));
    }

    /// <summary>
    /// Hides the run HUD. Prefer disabling the GameObject; fall back to CanvasGroup if provided.
    /// Safe to call from other scripts using the null-conditional pattern: RunHUDManager.Instance?.Hide();
    /// </summary>
    public void Hide()
    {
        if (runHUD != null)
        {
            runHUD.SetActive(false);
            return;
        }

        if (runHUDCanvasGroup != null)
        {
            runHUDCanvasGroup.alpha = 0f;
            runHUDCanvasGroup.interactable = false;
            runHUDCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Shows the run HUD (reverse of Hide).
    /// </summary>
    public void Show()
    {
        // 1. Turn the UI back on
        if (runHUD != null)
        {
            runHUD.SetActive(true);
        }
        else if (runHUDCanvasGroup != null)
        {
            runHUDCanvasGroup.alpha = 1f;
            runHUDCanvasGroup.interactable = true;
            runHUDCanvasGroup.blocksRaycasts = true;
        }

        // 2. Force a visual refresh to catch any stats gained while the HUD was hidden!
        if (RunManager.Instance != null)
        {
            UpdateGold(RunManager.Instance.Stats.CurrentGold);
            UpdateHealth(RunManager.Instance.Stats.PlayerHealth);
            UpdateDay(RunManager.Instance.Stats.CurrentDay);
            UpdateLevel(RunManager.Instance.Stats.PlayerLevel);
            UpdateReputation(RunManager.Instance.Stats.Experience);
            UpdateProvisionCap(RunManager.Instance.Stats.ProvisionCap);
        }
    }

    public void SlideOutAndHide(float duration = 0.5f)
    {
        if (gameObject.activeInHierarchy && hudRect != null)
        {
            StartCoroutine(SlideOutRoutine(duration));
        }
        else
        {
            Hide();
        }
    }

    private IEnumerator SlideOutRoutine(float duration)
    {
        Vector2 startPos = hudRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 300f); // Slide up by 300 pixels

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            hudRect.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        hudRect.anchoredPosition = endPos;
        Hide();
    }

    public void ResetAndShow()
    {
        if (hudRect != null)
        {
            hudRect.anchoredPosition = originalAnchoredPos; // Snap back instantly
        }
        Show();
    }
}