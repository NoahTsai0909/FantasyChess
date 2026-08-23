using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private Image unitIconImage; // Swapped from nameText
    [SerializeField] private TextMeshProUGUI valueText;
    [Header("Segment References")]
    [SerializeField] private RectTransform[] segmentRects; // Assign the 3 segment objects here
    [SerializeField] private Image[] segmentImages;

    [SerializeField] private Color playerColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color enemyColor = new Color(0.8f, 0.2f, 0.2f);

    public void UpdateRow(Sprite icon, int totalValue, float maxValue, List<(int val, Color color)> segmentData)
    {
        if (icon != null) unitIconImage.sprite = icon;
        valueText.text = totalValue.ToString();

        // 1. Sort the segments from highest contributor to lowest
        segmentData.Sort((a, b) => b.val.CompareTo(a.val));

        float currentOffset = 0f; // Tracks where the previous segment ended (left to right)

        for (int i = 0; i < segmentRects.Length; i++)
        {
            // If we have data for this slot and the value is greater than 0
            if (i < segmentData.Count && segmentData[i].val > 0)
            {
                segmentRects[i].gameObject.SetActive(true);
                segmentImages[i].color = segmentData[i].color;

                // Calculate what percentage of the MAX bar this segment takes up
                float pct = maxValue > 0 ? (float)segmentData[i].val / maxValue : 0f;

                // Use Unity's Anchor system to stretch the segment perfectly
                segmentRects[i].anchorMin = new Vector2(currentOffset, 0);
                segmentRects[i].anchorMax = new Vector2(currentOffset + pct, 1);

                // Zero out any offsets so it strictly obeys the anchors
                segmentRects[i].offsetMin = Vector2.zero;
                segmentRects[i].offsetMax = Vector2.zero;

                currentOffset += pct;
            }
            else
            {
                // Hide unused segments
                segmentRects[i].gameObject.SetActive(false);
            }
        }
    }
}