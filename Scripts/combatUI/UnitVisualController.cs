using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitVisualController : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("Glow Animation Settings")]
    public bool enablePulse = true;
    public float pulseSpeed = 6f;       // How fast it breathes
    public float minThickness = 2f;     // The thinnest the outline gets
    public float maxThickness = 7f;     // The thickest the outline gets

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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

        // Set the color and shader modes once
        sr.material.SetColor("_SolidOutline", outlineColor);
        sr.material.SetFloat("_OutlineEnabled", 1f);
        sr.material.SetFloat("_OutlineMode", 0f);
        sr.material.SetFloat("_OutlineShape", 0f);
    }

    private void Update()
    {
        // Every frame, if pulsing is enabled, smoothly animate the thickness
        if (enablePulse && sr != null && sr.material != null)
        {
            // Mathf.Sin returns -1 to 1. We map it to 0 to 1 for easier blending.
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

            // Blend smoothly between your min and max thickness based on the time
            float currentThickness = Mathf.Lerp(minThickness, maxThickness, timePulse);

            // Push the new thickness to the material
            sr.material.SetFloat("_Thickness", currentThickness);
        }
    }
}
