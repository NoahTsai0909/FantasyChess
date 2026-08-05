using UnityEngine;

public class RowContainerSnitch : MonoBehaviour
{
    private RectTransform rt;
    private Vector2 lastOffsetMin;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        lastOffsetMin = rt.offsetMin;
    }

    void LateUpdate()
    {
        // Check if the Left offset has changed from what it was...
        if (rt.offsetMin != lastOffsetMin)
        {
            // If the Left offset suddenly jumps to the crazy 255 value...
            if (Mathf.Abs(rt.offsetMin.x) > 200f || Mathf.Abs(rt.anchoredPosition.y) > 200f)
            {
                Debug.LogError($"<color=red>[SNITCH DETECTED HIJACK]</color> {gameObject.name} was just moved!\n" +
                               $"Left is now: {rt.offsetMin.x} | Pos Y is now: {rt.anchoredPosition.y}\n" +
                               $"EXACT CULPRIT:\n{StackTraceUtility.ExtractStackTrace()}");

                // Optional: Automatically pause the editor the moment the crime happens
                Debug.Break();
            }

            lastOffsetMin = rt.offsetMin;
        }
    }
}