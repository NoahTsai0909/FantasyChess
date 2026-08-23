using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPLinkHandler : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    private TextMeshProUGUI textComponent;
    private int currentLinkIndex = -1;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (TooltipUIManager.Instance == null) return;

        // Ask TextMeshPro if the mouse is currently over a link
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1)
        {
            // We just entered a new link
            if (linkIndex != currentLinkIndex)
            {
                currentLinkIndex = linkIndex;
                TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];

                // Extract the ID and tell the UI to show
                TooltipUIManager.Instance.Show(linkInfo.GetLinkID(), eventData.position);
            }
            else
            {
                // We are still hovering the same link, just update the position
                TooltipUIManager.Instance.UpdatePosition(eventData.position);
            }
        }
        else
        {
            // We moved off a link
            if (currentLinkIndex != -1)
            {
                currentLinkIndex = -1;
                TooltipUIManager.Instance.Hide();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // We moved completely off the text object
        currentLinkIndex = -1;
        if (TooltipUIManager.Instance != null)
        {
            TooltipUIManager.Instance.Hide();
        }
    }

    private void OnDisable()
    {
        // Automatically hide the tooltip if the text object is disabled while we are hovering it
        currentLinkIndex = -1;
        if (TooltipUIManager.Instance != null)
        {
            TooltipUIManager.Instance.Hide();
        }
    }
}
