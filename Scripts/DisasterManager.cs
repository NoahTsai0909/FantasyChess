using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisasterManager : MonoBehaviour
{
    [Header("Timing Settings")]
    [SerializeField] private float disasterStartTime = 20f; // When disaster starts
    [SerializeField] private float disasterTickInterval = 1f; // Damage every second
    [SerializeField] private int disasterInitialDamage = 1;

    [Header("Visual Settings")]
    [SerializeField] private Image screenOverlay; // Purple overlay image on Canvas
    [SerializeField] private Color disasterColor = new Color(0.5f, 0f, 0.5f, 0.3f); // Purple with alpha
    [SerializeField] private float fadeInDuration = 3f; // How long fade takes

    [Header("References")]
    [SerializeField] private gameManager combatManager;

    private float combatTimer = 0f;
    private bool disasterActive = false;
    private int currentDisasterTick = 0;
    private Coroutine disasterCoroutine;

    void Update()
    {
        if (combatManager == null) return;

        combatTimer += Time.deltaTime;

        // Start disaster when time reached
        if (!disasterActive && combatTimer >= disasterStartTime)
        {
            StartDisaster();
        }
    }

    private void StartDisaster()
    {
        disasterActive = true;
        currentDisasterTick = 0;

        // Start visual effects
        StartCoroutine(FadeInOverlay());

        // Start damage ticks
        disasterCoroutine = StartCoroutine(DisasterDamageRoutine());

        Debug.Log("DISASTER: The storm begins!");
    }

    private IEnumerator DisasterDamageRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(disasterTickInterval);

            currentDisasterTick++;
            int damageThisTick = disasterInitialDamage + currentDisasterTick - 1;

            DealDisasterDamage(damageThisTick);

            Debug.Log($"DISASTER: Dealt {damageThisTick} damage to all units");
        }
    }

    private void DealDisasterDamage(int damage)
    {
        // Damage all player units
        List<UnitInstance> playerUnits = combatManager.playerGrid.GetAllUnits();
        foreach (UnitInstance unit in playerUnits)
        {
            if (unit != null)
                unit.TakeDisasterDamage(damage);
        }

        // Damage all enemy units
        List<UnitInstance> enemyUnits = combatManager.enemyGrid.GetAllUnits();
        foreach (UnitInstance unit in enemyUnits)
        {
            if (unit != null)
                unit.TakeDisasterDamage(damage);
        }
    }

    private IEnumerator FadeInOverlay()
    {
        if (screenOverlay == null) yield break;

        screenOverlay.gameObject.SetActive(true);
        screenOverlay.color = new Color(disasterColor.r, disasterColor.g, disasterColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, disasterColor.a, elapsed / fadeInDuration);
            screenOverlay.color = new Color(disasterColor.r, disasterColor.g, disasterColor.b, alpha);
            yield return null;
        }

        screenOverlay.color = disasterColor;
    }

    public void StopDisaster()
    {
        if (disasterCoroutine != null)
        {
            StopCoroutine(disasterCoroutine);
            disasterCoroutine = null;
        }

        // Fade out overlay if combat ends quickly
        if (screenOverlay != null && screenOverlay.gameObject.activeSelf)
        {
            StartCoroutine(FadeOutOverlay());
        }

        disasterActive = false;
    }

    private IEnumerator FadeOutOverlay()
    {
        Color startColor = screenOverlay.color;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / 1f);
            screenOverlay.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        screenOverlay.gameObject.SetActive(false);
    }

    // Call this when combat ends to clean up
    private void OnCombatEnd()
    {
        StopDisaster();
    }

    void OnEnable()
    {
        CombatEventBus.OnCombatEvent += HandleCombatEvent;
    }

    void OnDisable()
    {
        CombatEventBus.OnCombatEvent -= HandleCombatEvent;
    }

    private void HandleCombatEvent(CombatEventBus.CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        if (type == CombatEventBus.CombatEventType.UnitDied)
        {
            // Check if combat ended via gameManager
            if (combatManager != null)
            {
                if (combatManager.isCombatActive() == false)
                {
                    OnCombatEnd();
                }
            }
        }
    }
}
