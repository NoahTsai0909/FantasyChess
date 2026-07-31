using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using static SceneLoader;

public class RunHUDManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private TMP_Text playerXPText;
    [SerializeField] private Image playerXPFill;
    [SerializeField] private TMP_Text provisionCapText;
    [SerializeField] private TMP_Text goldText;

    [Header("XP Settings")]
    [SerializeField] private int maxReputation = 10; // Max reputation for level up

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

    private void Start()
    {
        // Initialize with current values if RunManager exists
        StartCoroutine(InitializeFromRunManager());
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
            goldText.text = gold.ToString();
    }

    private void UpdateHealth(int health)
    {
        if (playerHealthText != null)
            playerHealthText.text = health.ToString();
    }

    private void UpdateDay(int day)
    {
        if (dayText != null)
            dayText.text = "Day \n" +day.ToString();
    }

    private void UpdateLevel(int level)
    {
        if (playerXPText != null)
            playerXPText.text = level.ToString();
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
            provisionCapText.text = cap.ToString();
    }
}