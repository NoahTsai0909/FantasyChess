using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private Image shieldFill;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI shieldText;

    public void SetValues(int currentHP, int maxHP, int shield)
    {
        healthFill.fillAmount = (float)currentHP / maxHP;
        shieldFill.fillAmount = Mathf.Clamp01((float)shield / maxHP);
    }

    public void SetHoverUIValues(int currentHP, int maxHP, int shield)
    {
        healthFill.fillAmount = (float)currentHP / maxHP;
        shieldFill.fillAmount = Mathf.Clamp01((float)shield / maxHP);

        if (healthText != null)
            healthText.text = currentHP.ToString();

        if (shieldText != null)
        {
            if (shield > 0)
            {
                shieldText.text = shield.ToString();
                shieldText.gameObject.SetActive(true);
            }
            else
            {
                shieldText.gameObject.SetActive(false);
            }
        }
    }

    public void SetTextVisible(bool visible)
        {
         if (healthText != null)
             healthText.gameObject.SetActive(visible);
    }
}
