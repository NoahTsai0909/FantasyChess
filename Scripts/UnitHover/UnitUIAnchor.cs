using UnityEngine;

[ExecuteAlways]
public class ForceAnchorFix : MonoBehaviour
{
    public float desiredWidth = 420f;

    void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            float canvasWidth = canvas.pixelRect.width;
            float horizontalPadding = (canvasWidth - desiredWidth) / 2f;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);

            // Set offsets to create a centered panel of desiredWidth
            rect.offsetMin = new Vector2(horizontalPadding, 0);
            rect.offsetMax = new Vector2(-horizontalPadding, 0);

            Debug.Log($"Set UI to {desiredWidth}px wide on {canvasWidth}px canvas");
        }

        DestroyImmediate(this);
    }
}