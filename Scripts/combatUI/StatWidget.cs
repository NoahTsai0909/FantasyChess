using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatWidget : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Set(Sprite icon, int value, bool isCrit)
    {
        iconImage.sprite = icon;
        if (isCrit)
        {
            valueText.text = value.ToString()+ "%";
        }
        else
        {
            valueText.text = value.ToString();
        }
    }
}
