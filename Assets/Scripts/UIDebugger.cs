using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIDebugger : MonoBehaviour
{
    [SerializeField] private RectTransform eventPanel;
    [SerializeField] private RectTransform buttonContainer;

    private Keyboard keyboard;

    void Start()
    {
        keyboard = Keyboard.current;

        Debug.Log($"EventPanel size: {eventPanel.rect.width}x{eventPanel.rect.height}");
        Debug.Log($"ButtonContainer size: {buttonContainer.rect.width}x{buttonContainer.rect.height}");

        if (eventPanel.GetComponent<VerticalLayoutGroup>())
            Debug.Log("EventPanel has VerticalLayoutGroup");
        if (buttonContainer.GetComponent<VerticalLayoutGroup>())
            Debug.Log("ButtonContainer has VerticalLayoutGroup");
    }

    void Update()
    {
        // Press F1 to log sizes (using new Input System)
        if (keyboard != null && keyboard.f1Key.wasPressedThisFrame)
        {
            Debug.Log($"EventPanel: {eventPanel.rect.size}");
            Debug.Log($"ButtonContainer: {buttonContainer.rect.size}");

            int childCount = buttonContainer.childCount;
            Debug.Log($"ButtonContainer has {childCount} children");

            for (int i = 0; i < childCount; i++)
            {
                RectTransform child = buttonContainer.GetChild(i) as RectTransform;
                if (child != null)
                    Debug.Log($"Child {i}: {child.rect.size}");
            }
        }
    }
}
