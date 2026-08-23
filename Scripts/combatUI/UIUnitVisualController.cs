using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIUnitVisualController : MonoBehaviour
{
    private Image image;
    private Vector3 originalScale;

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
        image = GetComponent<Image>();
        originalScale = transform.localScale;

        if (image.material != null)
        {
            image.material = new Material(image.material);
        }
    }

    public void InitializeVisuals(UnitDefinition def, Rarity rarity)
    {
        if (def != null && image != null)
        {
            image.sprite = def.unitSprite;
            UpdateRarityOutline(rarity);
        }
    }

    private void UpdateRarityOutline(Rarity rarity)
    {
        if (image == null || image.material == null) return;

        Color outlineColor = Color.gray;
        switch (rarity)
        {
            case Rarity.Uncommon: outlineColor = Color.green; break;
            case Rarity.Rare: outlineColor = Color.blue; break;
            case Rarity.Epic: ColorUtility.TryParseHtmlString("#A335EE", out outlineColor); break;
        }

        image.material.SetColor("_SolidOutline", outlineColor);
        image.material.SetFloat("_OutlineEnabled", 1f);
        image.material.SetFloat("_OutlineMode", 0f);
        image.material.SetFloat("_OutlineShape", 0f);
    }

    private void Update()
    {
        // 1. Outline Pulse
        if (enablePulse && image != null && image.material != null)
        {
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float currentThickness = Mathf.Lerp(minThickness, maxThickness, timePulse);
            image.material.SetFloat("_Thickness", currentThickness);
        }

        // 2. Idle Breathing
        if (enableBreathing)
        {
            float breathe = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;
            transform.localScale = new Vector3(originalScale.x, originalScale.y + breathe, originalScale.z);
        }
    }
}
