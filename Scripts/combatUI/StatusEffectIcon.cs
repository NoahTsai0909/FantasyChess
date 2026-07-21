using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI stackText;

    private StatusEffectType type;

    // We now pass the specific Sprite in the Initialize method
    public void Initialize(StatusEffectType type, Sprite customSprite)
    {
        this.type = type;
        iconImage.sprite = customSprite;
        iconImage.color = Color.white; // Ensures the image uses its original colors
    }

    public void SetStacks(int stacks)
    {
        if (stacks <= 0)
            return;

        stackText.text = stacks.ToString();
        stackText.gameObject.SetActive(true);
    }
}

