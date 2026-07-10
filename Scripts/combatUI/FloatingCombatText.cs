using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class FloatingCombatText : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve;

    private float timer;
    private Color baseColor;

    public void Initialize(string value, Color color, bool isCrit)
    {
        text.text = value;
        text.color = color;
        baseColor = color;

        if (isCrit)
        {
            text.text += "!";
            text.fontStyle = FontStyles.Bold;
            text.fontSize *= 1.4f;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        float t = timer / lifetime;
        Color c = baseColor;
        c.a = fadeCurve.Evaluate(t);
        text.color = c;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}

