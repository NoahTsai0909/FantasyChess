using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitVisualController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalSpriteColor;
    private Vector3 originalScale; // NEW: Track the base scale

    private Coroutine activeAnimationRoutine; // Track if we are attacking/getting hit

    [Header("Glow Animation Settings")]
    public bool enablePulse = true;
    public float pulseSpeed = 4f;
    public float minThickness = 2f;
    public float maxThickness = 6f;

    [Header("Juice Settings")]
    public bool enableBreathing = true;
    public float breatheSpeed = 3f;
    public float breatheAmount = 0.03f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSpriteColor = sr.color;
        originalScale = transform.localScale; // Store base scale on awake
    }

    public void InitializeVisuals(UnitDefinition def)
    {
        if (def != null && sr != null)
        {
            sr.sprite = def.unitSprite;
        }
    }

    public void SetDirection(bool isPlayerSide)
    {
        if (sr != null)
        {
            sr.flipX = !isPlayerSide;
        }
    }

    public void UpdateRarityOutline(Rarity rarity)
    {
        if (sr == null || sr.material == null) return;

        Color outlineColor = Color.gray;
        switch (rarity)
        {
            case Rarity.Uncommon: outlineColor = Color.green; break;
            case Rarity.Rare: outlineColor = Color.blue; break;
            case Rarity.Epic: ColorUtility.TryParseHtmlString("#A335EE", out outlineColor); break;
        }

        sr.material.SetColor("_SolidOutline", outlineColor);
        sr.material.SetFloat("_OutlineEnabled", 1f);
        sr.material.SetFloat("_OutlineMode", 0f);
        sr.material.SetFloat("_OutlineShape", 0f);
    }

    private void Update()
    {
        // 1. Outline Pulse
        if (enablePulse && sr != null && sr.material != null)
        {
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float currentThickness = Mathf.Lerp(minThickness, maxThickness, timePulse);
            sr.material.SetFloat("_Thickness", currentThickness);
        }

        // 2. Idle Breathing (Only breathe if we aren't currently attacking or getting hit!)
        if (enableBreathing && activeAnimationRoutine == null)
        {
            // A simple sine wave that slightly stretches the Y axis
            float breathe = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;
            transform.localScale = new Vector3(originalScale.x, originalScale.y + breathe, originalScale.z);
        }
    }

    /* =========================
     * JUICE ANIMATIONS
     * ========================= */

    public void Flash(Color flashColor)
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (activeAnimationRoutine != null)
        {
            StopCoroutine(activeAnimationRoutine);
            // SAFETY RESET: Fix the scale in case we interrupted an Attack Windup!
            transform.localScale = originalScale;
        }

        activeAnimationRoutine = StartCoroutine(HitReactionRoutine(flashColor));
    }

    // Trigger this when the unit uses an ability
    public void PlayAttackAnimation()
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (activeAnimationRoutine != null)
        {
            StopCoroutine(activeAnimationRoutine);
            // SAFETY RESET: Fix the color in case we interrupted a Damage/Heal Flash!
            sr.color = originalSpriteColor;
        }

        activeAnimationRoutine = StartCoroutine(AttackSnapRoutine());
    }

    private IEnumerator HitReactionRoutine(Color flashColor)
    {
        // 1. The Impact (Instant squash and color change)
        sr.color = flashColor;
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z); // Flatten out

        float recoverTime = 0.2f;
        float timer = 0f;

        // 2. The Recovery (Smoothly bounce back to normal)
        while (timer < recoverTime)
        {
            timer += Time.deltaTime;
            float t = timer / recoverTime;

            // Lerp scale back to normal
            transform.localScale = Vector3.Lerp(new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z), originalScale, t);

            // End the color flash very early in the animation
            if (timer > 0.05f) sr.color = originalSpriteColor;

            yield return null;
        }

        transform.localScale = originalScale;
        sr.color = originalSpriteColor;
        activeAnimationRoutine = null;
    }

    private IEnumerator AttackSnapRoutine()
    {
        // 1. Windup (Squash down and prepare)
        float windupTime = 0.1f;
        float timer = 0f;
        Vector3 windupScale = new Vector3(originalScale.x * 1.15f, originalScale.y * 0.85f, originalScale.z);

        while (timer < windupTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, windupScale, timer / windupTime);
            yield return null;
        }

        // 2. The Strike (Snap extremely tall and skinny, completely in place)
        float strikeTime = 0.05f;
        timer = 0f;
        Vector3 strikeScale = new Vector3(originalScale.x * 0.8f, originalScale.y * 1.25f, originalScale.z);

        while (timer < strikeTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(windupScale, strikeScale, timer / strikeTime);
            yield return null;
        }

        // 3. Recover
        float recoverTime = 0.15f;
        timer = 0f;
        while (timer < recoverTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(strikeScale, originalScale, timer / recoverTime);
            yield return null;
        }

        transform.localScale = originalScale;
        activeAnimationRoutine = null;
    }

    /* =========================
     * DEATH
     * ========================= */

    public void PlayDeathAnimationAndDestroy()
    {
        if (activeAnimationRoutine != null) StopCoroutine(activeAnimationRoutine);
        StartCoroutine(VisualDeathRoutine());
    }

    private IEnumerator VisualDeathRoutine()
    {
        if (sr != null)
        {
            Color startColor = sr.color;
            Color targetColor = new Color(0.3f, 0.3f, 0.3f, 0f);
            float fadeDuration = 0.5f;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                sr.color = Color.Lerp(startColor, targetColor, timer / fadeDuration);

                if (sr.material != null)
                {
                    sr.material.SetFloat("_Thickness", Mathf.Lerp(maxThickness, 0f, timer / fadeDuration));
                }
                yield return null;
            }
        }
        Destroy(gameObject);
    }

    public void SetBaseScale(Vector3 newScale)
    {
        originalScale = newScale;
        transform.localScale = newScale;
    }
}