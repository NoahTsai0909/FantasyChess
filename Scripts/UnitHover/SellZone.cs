using UnityEngine;
using UnityEngine.UI;

public class SellZone : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.red;
    private Color originalColor;
    private Image image; // Changed from SpriteRenderer to Image

    void Start()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
        }
    }

    public void Highlight(bool highlight)
    {
        if (image != null)
        {
            image.color = highlight ? highlightColor : originalColor;
        }
    }
}
