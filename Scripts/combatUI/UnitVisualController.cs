using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitVisualController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalSpriteColor;
    private Coroutine flashCoroutine;

    [Header("Glow Animation Settings")]
    public bool enablePulse = true;
    public float pulseSpeed = 4f;
    public float minThickness = 2f;
    public float maxThickness = 6f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSpriteColor = sr.color; // Store the base color immediately
    }

    // Called once when the unit first sets up
    public void InitializeVisuals(UnitDefinition def)
    {
        if (def != null && sr != null)
        {
            sr.sprite = def.unitSprite;
        }
    }

    // Handles which way the sprite faces
    public void SetDirection(bool isPlayerSide)
    {
        if (sr != null)
        {
            sr.flipX = !isPlayerSide;
        }
    }

    // Sets the shader outline
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

    // Replaces the Flash() method inside UnitInstance
    public void Flash(Color flashColor)
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine(flashColor));
    }

    private IEnumerator FlashRoutine(Color flashColor)
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalSpriteColor;
        flashCoroutine = null;
    }

    private void Update()
    {
        if (enablePulse && sr != null && sr.material != null)
        {
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float currentThickness = Mathf.Lerp(minThickness, maxThickness, timePulse);
            sr.material.SetFloat("_Thickness", currentThickness);
        }
    }
}
