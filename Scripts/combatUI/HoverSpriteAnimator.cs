using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class HoverSpriteAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [Tooltip("Drag the sliced frames here in order (Closed to Open)")]
    [SerializeField] private Sprite[] frames;

    [Tooltip("How fast the eye opens and closes")]
    [SerializeField] private float framesPerSecond = 12f;

    private Image targetImage;
    private float currentFrameIndex = 0f;
    private bool isHovered = false;

    private void Awake()
    {
        targetImage = GetComponent<Image>();

        // Ensure it starts completely closed
        if (frames != null && frames.Length > 0)
        {
            targetImage.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;

        if (isHovered)
        {
            // Play forward (Opening)
            currentFrameIndex += Time.deltaTime * framesPerSecond;

            // Clamp at the final frame
            if (currentFrameIndex >= frames.Length - 1)
            {
                currentFrameIndex = frames.Length - 1;
            }
        }
        else
        {
            // Play in reverse (Closing)
            currentFrameIndex -= Time.deltaTime * framesPerSecond;

            // Clamp at the first frame
            if (currentFrameIndex <= 0)
            {
                currentFrameIndex = 0;
            }
        }

        // Apply the mathematically calculated frame to the Image
        targetImage.sprite = frames[Mathf.FloorToInt(currentFrameIndex)];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void OnDisable()
    {
        // Safety catch: if the button gets turned off, reset its state
        isHovered = false;
        currentFrameIndex = 0f;
        if (frames != null && frames.Length > 0 && targetImage != null)
        {
            targetImage.sprite = frames[0];
        }
    }
}
