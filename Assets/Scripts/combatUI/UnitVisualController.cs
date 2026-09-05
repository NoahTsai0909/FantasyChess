using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitVisualController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalSpriteColor;
    private Vector3 originalScale;
    private Vector3 originalLocalPosition;
    private bool isPlayer = true;
    [Header("Shadow Settings")]
    public Sprite shadowSprite;

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

    private SpriteRenderer mutationRuneSR;
    private GameObject activeParticles;


    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSpriteColor = sr.color;
        originalScale = transform.localScale;
        originalLocalPosition = transform.localPosition;

        if (shadowSprite != null)
        {
            GameObject shadow = new GameObject("DropShadow");
            shadow.transform.SetParent(this.transform, false);

            shadow.transform.localPosition = Vector3.zero;

            shadow.transform.localScale = Vector3.one;

            SpriteRenderer shadowSR = shadow.AddComponent<SpriteRenderer>();
            shadowSR.sprite = shadowSprite;

            shadowSR.color = new Color(1f, 1f, 1f, 0.6f);

            shadowSR.sortingOrder = sr.sortingOrder - 1;
        }
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
        isPlayer = isPlayerSide;
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
    }

    private void Update()
    {

        if (enablePulse && sr != null && sr.material != null)
        {
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float currentThickness = Mathf.Lerp(minThickness, maxThickness, timePulse);
            sr.material.SetFloat("_Thickness", currentThickness);
        }

        if (enableBreathing && activeAnimationRoutine == null)
        {

            float breathe = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;

            transform.localScale = new Vector3(originalScale.x - (breathe * 0.5f), originalScale.y + breathe, originalScale.z);
        }

        if (mutationRuneSR != null)
        {
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            Color c = mutationRuneSR.color;
            c.a = Mathf.Lerp(0.3f, 0.8f, timePulse);
            mutationRuneSR.color = c;
        }
    }


    public void Flash(Color flashColor, bool doKnockback = true)
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (activeAnimationRoutine != null)
        {
            StopCoroutine(activeAnimationRoutine);
            transform.localScale = originalScale;
            transform.localPosition = originalLocalPosition;
        }
        else
        {
            originalLocalPosition = transform.localPosition;
        }

        activeAnimationRoutine = StartCoroutine(HitReactionRoutine(flashColor, doKnockback));
    }

    public void PlayAttackAnimation()
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (activeAnimationRoutine != null)
        {
            StopCoroutine(activeAnimationRoutine);
            sr.color = originalSpriteColor;
            transform.localScale = originalScale;
            transform.localPosition = originalLocalPosition;
        }
        else
        {
            originalLocalPosition = transform.localPosition;
        }

        activeAnimationRoutine = StartCoroutine(AttackSnapRoutine());
    }

    private IEnumerator HitReactionRoutine(Color flashColor, bool doKnockback)
    {
        sr.color = flashColor;

        Vector3 originalPos = originalLocalPosition;
        Vector3 knockbackPos = originalPos;

        if (doKnockback)
        {
            float knockbackDirection = isPlayer ? -0.5f : 0.5f;
            knockbackPos = originalPos + new Vector3(knockbackDirection, 0f, 0f);
        }

        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z);
        transform.localPosition = knockbackPos;

        float recoverTime = 0.5f;
        float timer = 0f;

        // 2. The Recovery
        while (timer < recoverTime)
        {
            timer += Time.deltaTime;
            float t = timer / recoverTime;

            transform.localScale = Vector3.Lerp(new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z), originalScale, t);

            if (doKnockback)
            {
                transform.localPosition = Vector3.Lerp(knockbackPos, originalPos, t);
            }

            if (timer > 0.05f) sr.color = originalSpriteColor;

            yield return null;
        }

        transform.localScale = originalScale;
        transform.localPosition = originalPos; // Ensure position resets
        sr.color = originalSpriteColor;
        activeAnimationRoutine = null;
    }

    private IEnumerator AttackSnapRoutine()
    {
        float windupTime = 0.1f;
        float timer = 0f;
        Vector3 windupScale = new Vector3(originalScale.x * 1.15f, originalScale.y * 0.85f, originalScale.z);

        while (timer < windupTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, windupScale, timer / windupTime);
            yield return null;
        }

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

    public void ApplyMutationVisuals(MutationPrefixSO prefix)
    {
        if (prefix == null) return;

        if (prefix.runeSprite != null)
        {
            if (mutationRuneSR == null)
            {
                GameObject runeObj = new GameObject("MutationRune");
                runeObj.transform.SetParent(this.transform, false);

                runeObj.transform.localPosition = new Vector3(0f, -0.4f, 0f);

                mutationRuneSR = runeObj.AddComponent<SpriteRenderer>();
                mutationRuneSR.sortingOrder = sr.sortingOrder - 1;
            }
            mutationRuneSR.sprite = prefix.runeSprite;
            mutationRuneSR.color = prefix.runeColor;
        }

        if (prefix.particlePrefab != null)
        {
            if (activeParticles != null) Destroy(activeParticles);

            Transform particleParent = mutationRuneSR != null ? mutationRuneSR.transform : transform;
            activeParticles = Instantiate(prefix.particlePrefab, particleParent);

            activeParticles.transform.localPosition = new Vector3(0f, -0.4f, 0f); 

            ParticleSystem ps = activeParticles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;

                Color particleTint = prefix.runeColor;

                float glowMultiplier = 1.5f; 
                particleTint.r = Mathf.Clamp01(particleTint.r * glowMultiplier);
                particleTint.g = Mathf.Clamp01(particleTint.g * glowMultiplier);
                particleTint.b = Mathf.Clamp01(particleTint.b * glowMultiplier);
                particleTint.a = 1f;

                main.startColor = particleTint;

                ParticleSystemRenderer psRenderer = activeParticles.GetComponent<ParticleSystemRenderer>();
                if (psRenderer != null && sr != null)
                {
                    psRenderer.sortingLayerID = sr.sortingLayerID;
                    psRenderer.sortingOrder = sr.sortingOrder + 1;
                }
            }
        }
    }
}