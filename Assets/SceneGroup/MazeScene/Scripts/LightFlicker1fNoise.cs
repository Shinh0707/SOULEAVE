using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker1fNoise : MonoBehaviour
{
    [SerializeField] public Light2D targetLight;

    [SerializeField] private float baseRange = 10f;
    [SerializeField] private float baseIntensity = 1f;

    [SerializeField] private float rangeFlickerAmount = 0.5f;
    [SerializeField] private float intensityFlickerAmount = 0.2f;

    [SerializeField] private int octaves = 4;
    [SerializeField] private float lacunarity = 2f;
    [SerializeField] private float gain = 0.5f;
    [SerializeField] private float timeScale = 1f; // 新しいパラメーター: 時間スケール

    private float[] frequencies;
    private float[] amplitudes;
    private float[] phases;

    public float BaseRange
    {
        get { return baseRange; }
        set
        {
            baseRange = value;
            if (targetLight != null) targetLight.pointLightOuterRadius = value;
        }
    }

    public float BaseIntensity
    {
        get { return baseIntensity; }
        set
        {
            baseIntensity = value;
            if (targetLight != null) targetLight.intensity = value;
        }
    }

    private void Start()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light2D>();
            if (targetLight == null)
            {
                Debug.LogError("No Light component found!");
                enabled = false;
                return;
            }
        }

        InitializeNoiseParameters();
    }

    private void InitializeNoiseParameters()
    {
        frequencies = new float[octaves];
        amplitudes = new float[octaves];
        phases = new float[octaves];

        for (int i = 0; i < octaves; i++)
        {
            frequencies[i] = Mathf.Pow(lacunarity, i);
            amplitudes[i] = Mathf.Pow(gain, i);
            phases[i] = Random.Range(0f, 2f * Mathf.PI);
        }
    }

    private void FixedUpdate()
    {
        float time = Time.time * timeScale;
        float rangeNoise = Generate1fNoise(time);
        float intensityNoise = Generate1fNoise(time + 1000f); // Offset to get different noise

        targetLight.pointLightOuterRadius = baseRange*(1.0f+rangeNoise*rangeFlickerAmount);// * rangeFlickerAmount;
        targetLight.intensity = baseIntensity * (1.0f + intensityNoise * intensityFlickerAmount);// * intensityFlickerAmount;
    }

    private float Generate1fNoise(float t)
    {
        float noise = 0f;
        for (int i = 0; i < octaves; i++)
        {
            noise += amplitudes[i] * Mathf.Sin(frequencies[i] * t + phases[i]);
        }
        return noise / octaves;
    }
}