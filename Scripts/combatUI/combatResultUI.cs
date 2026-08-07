using System.Collections;
using UnityEngine;

public class CombatResultUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform victoryBannerRect;
    [SerializeField] private RectTransform defeatBannerRect;
    [SerializeField] private ParticleSystem sparksParticles;

    [Header("Animation Settings")]
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private float lingerDuration = 1.5f;
    [SerializeField] private float floatSpeed = 3f;
    [SerializeField] private float floatAmplitude = 10f;

    private RectTransform activeBannerRect;
    private Vector2 originalBannerPos;
    private bool isFloating = false;

    private void Awake()
    {
        // Ensure starting state is hidden
        panelCanvasGroup.alpha = 0f;

        victoryBannerRect.gameObject.SetActive(false);
        defeatBannerRect.gameObject.SetActive(false);
    }

    public void ShowResult(bool isVictory)
    {
        gameObject.SetActive(true);

        // Choose the correct banner
        victoryBannerRect.gameObject.SetActive(isVictory);
        defeatBannerRect.gameObject.SetActive(!isVictory);

        activeBannerRect = isVictory ? victoryBannerRect : defeatBannerRect;

        activeBannerRect.localScale = Vector3.zero;
        originalBannerPos = activeBannerRect.anchoredPosition;

        StartCoroutine(AnimateBannerSequence());
    }

    private IEnumerator AnimateBannerSequence()
    {
        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 0.8f, elapsed / 0.15f);
            yield return null;
        }

        if (sparksParticles != null) sparksParticles.Play();

        elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            float scale = Mathf.Sin(t * Mathf.PI * 0.6f) * 1.15f;
            if (t > 0.6f) scale = Mathf.Lerp(1.15f, 1.0f, (t - 0.6f) * 2.5f);

            activeBannerRect.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        activeBannerRect.localScale = Vector3.one;

        isFloating = true;
        yield return new WaitForSeconds(lingerDuration);
        isFloating = false;

        elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.15f;
            float easeIn = t * t;

            activeBannerRect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, easeIn);
            panelCanvasGroup.alpha = Mathf.Lerp(0.8f, 0f, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isFloating && activeBannerRect != null)
        {
            float newY = originalBannerPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            activeBannerRect.anchoredPosition = new Vector2(originalBannerPos.x, newY);
        }
    }
}