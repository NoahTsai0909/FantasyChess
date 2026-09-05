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

    private Image mutationRuneImage;

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
        if (enablePulse && image != null && image.material != null)
        {
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float currentThickness = Mathf.Lerp(minThickness, maxThickness, timePulse);
            image.material.SetFloat("_Thickness", currentThickness);
        }

        if (enableBreathing)
        {
            float breathe = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;
            transform.localScale = new Vector3(originalScale.x, originalScale.y + breathe, originalScale.z);
        }

        if (mutationRuneImage != null)
        {
            float timePulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            Color c = mutationRuneImage.color;
            c.a = Mathf.Lerp(0.3f, 0.8f, timePulse);
            mutationRuneImage.color = c;


            RectTransform myRT = GetComponent<RectTransform>();

            mutationRuneImage.rectTransform.position = myRT.position;

            mutationRuneImage.rectTransform.localPosition += new Vector3(0, -50f, 0);
        }
    }

    public void ApplyMutationVisuals(MutationPrefixSO prefix)
    {
        if (prefix == null || prefix.runeSprite == null) return;

        if (mutationRuneImage == null)
        {
            GameObject runeObj = new GameObject("MutationRuneUI");

            runeObj.transform.SetParent(this.transform.parent, false);

            runeObj.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

            mutationRuneImage = runeObj.AddComponent<Image>();
            mutationRuneImage.raycastTarget = false;

            LayoutElement le = runeObj.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            RectTransform rt = runeObj.GetComponent<RectTransform>();
            RectTransform myRT = GetComponent<RectTransform>();

            rt.anchorMin = myRT.anchorMin;
            rt.anchorMax = myRT.anchorMax;
            rt.pivot = myRT.pivot;
            rt.sizeDelta = new Vector2(120f, 120f);
        }

        mutationRuneImage.sprite = prefix.runeSprite;
        mutationRuneImage.color = prefix.runeColor;
    }

    private void OnDestroy()
    {
        if (mutationRuneImage != null)
        {
            Destroy(mutationRuneImage.gameObject);
        }
    }
}
