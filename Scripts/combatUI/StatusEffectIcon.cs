using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI stackText;

    private StatusEffectType type;

    public void Initialize(StatusEffectType type)
    {
        this.type = type;
        iconImage.color = GetColorForType(type);
    }

    public void SetStacks(int stacks)
    {
        if (stacks <= 0)
            return;

        stackText.text = stacks.ToString();
        stackText.gameObject.SetActive(true);
    }

    private Color GetColorForType(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Burn => new Color(1f, 0.5f, 0.1f),   // orange
            StatusEffectType.Poison => new Color(0.6f, 0.2f, 0.8f), // purple
            StatusEffectType.Haste => new Color(0.4f, 1f, 0.4f),   // light green
            StatusEffectType.Slow => new Color(0.55f, 0.4f, 0.2f),// brown
            StatusEffectType.Freeze => new Color(0.6f, 0.8f, 1f),   // light blue
            _ => Color.white
        };
    }
}

