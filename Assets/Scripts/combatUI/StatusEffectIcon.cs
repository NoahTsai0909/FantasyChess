using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 

public class StatusEffectIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI stackText;

    private StatusEffectType type;
    private int currentStacks = -1;

    public void Initialize(StatusEffectType type, Sprite customSprite)
    {
        this.type = type;
        iconImage.sprite = customSprite;
        iconImage.color = Color.white;
    }

    public void SetStacks(int stacks)
    {
        if (stacks <= 0)
            return;

        if (currentStacks > 0 && stacks != currentStacks)
        {
            transform.DOKill(true);

            transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0f), 0.3f, 5, 0.5f).SetLink(gameObject);
        }

        currentStacks = stacks;

        stackText.text = stacks.ToString();
        stackText.gameObject.SetActive(true);
    }
}

