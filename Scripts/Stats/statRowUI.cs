using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private Image unitIconImage; // Swapped from nameText
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Image fillBar;

    [SerializeField] private Color playerColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color enemyColor = new Color(0.8f, 0.2f, 0.2f);

    public void UpdateRow(Sprite icon, int value, float maxValue, bool isPlayer)
    {
        // Apply the sprite (if it exists)
        if (icon != null) unitIconImage.sprite = icon;

        valueText.text = value.ToString();
        fillBar.fillAmount = maxValue > 0 ? (float)value / maxValue : 0f;
        fillBar.color = isPlayer ? playerColor : enemyColor;
    }
}