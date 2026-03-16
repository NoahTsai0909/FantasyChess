using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CooldownBarUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TextMeshProUGUI valueText;

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