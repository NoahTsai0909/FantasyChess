using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatWidget : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Set(Sprite icon, int value)
    {
        iconImage.sprite = icon;
        valueText.text = value.ToString();
    }
}
