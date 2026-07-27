using UnityEngine;
using UnityEngine.UI;

public class SceneEntryFader : MonoBehaviour
{
    [SerializeField] private Image faderImage;
    [SerializeField] private float fadeDuration = 0.5f;

    // Awake runs before the very first frame is rendered
    private void Awake()
    {
        if (faderImage != null)
        {
            // Instantly snap the image to solid black before the player's screen turns on
            faderImage.color = new Color(0, 0, 0, 1);
            faderImage.gameObject.SetActive(true);
        }
    }

    // Start runs right as the game begins playing
    private void Start()
    {
        if (faderImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            faderImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Disable it once the fade is done so it doesn't block your UI clicks
        faderImage.gameObject.SetActive(false);
    }
}
