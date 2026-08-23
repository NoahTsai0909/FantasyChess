using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image cooldownFill;
    [SerializeField] private Image backgroundImage; // Links to your Background image
    [SerializeField] private TextMeshProUGUI valueText;

    [Header("Common Visuals")]
    [SerializeField] private Sprite commonBackground;
    [SerializeField] private Sprite commonFill;

    [Header("Uncommon Visuals")]
    [SerializeField] private Sprite uncommonBackground;
    [SerializeField] private Sprite uncommonFill;

    [Header("Rare Visuals")]
    [SerializeField] private Sprite rareBackground;
    [SerializeField] private Sprite rareFill;

    [Header("Epic Visuals")]
    [SerializeField] private Sprite epicBackground;
    [SerializeField] private Sprite epicFill;

    // Swaps the sprites based on the passed rarity
    public void SetVisuals(Rarity rarity)
    {
        if (cooldownFill == null || backgroundImage == null) return;

        switch (rarity)
        {
            case Rarity.Common:
                backgroundImage.sprite = commonBackground;
                cooldownFill.sprite = commonFill;
                break;
            case Rarity.Uncommon:
                backgroundImage.sprite = uncommonBackground;
                cooldownFill.sprite = uncommonFill;
                break;
            case Rarity.Rare:
                backgroundImage.sprite = rareBackground;
                cooldownFill.sprite = rareFill;
                break;
            case Rarity.Epic:
                backgroundImage.sprite = epicBackground;
                cooldownFill.sprite = epicFill;
                break;
        }
    }

    public void SetValues(float remaining, float maxCooldown)
    {
        float fill = remaining / maxCooldown;
        cooldownFill.fillAmount = fill;

        if (valueText != null)
        {
            valueText.text = remaining.ToString("F1") + "s";
        }
    }

    public void SetTextVisible(bool visible)
    {
        if (valueText != null)
            valueText.gameObject.SetActive(visible);
    }
}