using TMPro;
using UnityEngine;
using DG.Tweening;

public class FloatingCombatText : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float lifetime = 1.0f;

    public void Initialize(string value, Color color, bool isCrit)
    {
        text.SetText(TextIconUtility.ParseDescription(value));
        text.color = color;

        if (isCrit)
        {
            text.text += "!";
            text.fontStyle = FontStyles.Bold;
            text.fontSize *= 1.4f;
        }

        Sequence seq = DOTween.Sequence();

        // 1. Calculate the scatter destination
        // Pick a random distance left or right (between 0.5 and 1.5 units away)
        float randomX = Random.Range(0.5f, 1.5f) * (Random.value > 0.5f ? 1f : -1f);

        float floorY = transform.position.y - 0.5f;

        Vector3 landingSpot = new Vector3(transform.position.x + randomX, floorY, transform.position.z);

        if (isCrit)
        {
            transform.localScale = Vector3.zero;
            seq.Append(transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(transform.DOScale(1f, 0.1f));
        }

        // DOJump(Target, JumpPower, NumberOfBounces, Duration)
        seq.Insert(0f, transform.DOJump(landingSpot, 1.5f, 2, lifetime).SetEase(Ease.Linear));

        // 4. The Fade Out
        seq.Insert(lifetime * 0.5f, text.DOFade(0f, lifetime * 0.5f).SetEase(Ease.InQuad));

        // 5. Clean up the object when the timeline finishes
        seq.OnComplete(() => Destroy(gameObject));
    }
}

