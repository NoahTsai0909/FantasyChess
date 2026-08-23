using UnityEngine;
using System.Collections;

public class UIShineSweep : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Sweep Settings")]
    public float startX = -250f;
    public float endX = 250f;
    public float sweepDuration = 0.6f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Animation Feel")]
    [Tooltip("Controls the speed of the sweep. Time is on the X axis (0 to 1), Position is on the Y axis (0 to 1).")]
    public AnimationCurve sweepCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        startPos.x = startX;
        rectTransform.anchoredPosition = startPos;

        StartCoroutine(SweepRoutine());
    }

    private IEnumerator SweepRoutine()
    {
        while (true)
        {
            float wait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(wait);

            float elapsedTime = 0f;
            Vector2 currentPos = rectTransform.anchoredPosition;

            while (elapsedTime < sweepDuration)
            {
                elapsedTime += Time.deltaTime;

                // Calculate our linear progress (0.0 to 1.0)
                float timeRatio = Mathf.Clamp01(elapsedTime / sweepDuration);

                // Feed that time into your custom curve to get the eased position
                float curveEvaluation = sweepCurve.Evaluate(timeRatio);

                currentPos.x = Mathf.LerpUnclamped(startX, endX, curveEvaluation);
                rectTransform.anchoredPosition = currentPos;

                yield return null;
            }

            currentPos.x = startX;
            rectTransform.anchoredPosition = currentPos;
        }
    }
}
