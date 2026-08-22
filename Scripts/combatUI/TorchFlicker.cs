using UnityEngine;
using UnityEngine.Rendering.Universal; // Needed for 2D Lights!

[RequireComponent(typeof(Light2D))]
public class TorchFlicker : MonoBehaviour
{
    private Light2D torchLight;

    [Header("Intensity Settings")]
    public float baseIntensity = 1f;
    public float intensityJitter = 0.3f;

    [Header("Radius Settings")]
    public float baseRadius = 5f;
    public float radiusJitter = 0.5f;

    [Header("Speed")]
    public float flickerSpeed = 3f;

    private float randomOffset;

    void Awake()
    {
        torchLight = GetComponent<Light2D>();

        // This ensures if you have 5 torches on screen, they don't all pulse at the exact same time!
        randomOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // Generate a smooth wave between 0.0 and 1.0 based on time
        float noise = Mathf.PerlinNoise((Time.time + randomOffset) * flickerSpeed, 0f);

        // Convert that 0 to 1 range into a -1 to 1 range so it fluctuates up AND down
        float mappedNoise = (noise - 0.5f) * 2f;

        // Apply the jitter to both the brightness and the physical size of the light
        torchLight.intensity = baseIntensity + (mappedNoise * intensityJitter);
        torchLight.pointLightOuterRadius = baseRadius + (mappedNoise * radiusJitter);
    }
}