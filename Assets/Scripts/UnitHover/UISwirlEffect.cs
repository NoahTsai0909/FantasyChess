using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISwirlEffect : MonoBehaviour
{
    private Image maskImage;
    private Material runtimeMaterial;

    [SerializeField] private float swirlSpeed = 0.5f;
    [SerializeField] private float distortionAmount = 0.05f;

    void Start()
    {
        maskImage = GetComponent<Image>();

        // We must create an instance of the material so we don't permanently change the source file
        if (maskImage.material != null)
        {
            runtimeMaterial = new Material(maskImage.material);
            maskImage.material = runtimeMaterial;
        }
    }

    void Update()
    {
        if (runtimeMaterial != null)
        {
            // Calculate a slow, wandering offset
            float offsetX = Mathf.Sin(Time.time * swirlSpeed) * distortionAmount;
            float offsetY = Mathf.Cos(Time.time * swirlSpeed * 0.8f) * distortionAmount;

            // Apply the offset to the texture coordinates
            runtimeMaterial.mainTextureOffset = new Vector2(offsetX, offsetY);
        }
    }

    void OnDestroy()
    {
        // Clean up the instantiated material to prevent memory leaks
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}