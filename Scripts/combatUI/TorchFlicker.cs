using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        randomOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise((Time.time + randomOffset) * flickerSpeed, 0f);

        float mappedNoise = (noise - 0.5f) * 2f;

        torchLight.intensity = baseIntensity + (mappedNoise * intensityJitter);
        torchLight.pointLightOuterRadius = baseRadius + (mappedNoise * radiusJitter);
    }
}