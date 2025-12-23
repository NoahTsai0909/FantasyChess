// Update your SellZone.cs back to using SpriteRenderer
using UnityEngine;

public class SellZone : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.red;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Highlight(bool highlight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlight ? highlightColor : originalColor;
        }
    }
}
