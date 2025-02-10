using UnityEngine;

public class NIghtDayCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 10f;
    public float startTime = 16f;
    private float currentTime;

    [Header("Sun Settings")]
    public Light sun;
    public float sunBaseIntensity = 1f;
    public float sunVariation = 1.5f;
    public Gradient sunColor;

    [Header("Night Settings")]
    [Range(0f, 1f)]
    public float nightDarkness = 0.1f; // Controls how dark nights are (lower = darker)
    public float nightFadeSpeed = 1f; // How quickly it transitions to night

    [Header("Lighting Settings")]
    public Color dayAmbientLight = new Color(0.7f, 0.7f, 0.7f);
    public Color nightAmbientLight = new Color(0.05f, 0.05f, 0.1f); // Made darker

    [Header("Skybox References")]
    public Material skyboxMaterial;

    private void OnEnable()
    {
        if (sun == null)
            sun = GetComponent<Light>();

        if (RenderSettings.skybox != null)
            skyboxMaterial = RenderSettings.skybox;
    }

    private void Start()
    {
        currentTime = startTime;
        UpdateLighting(currentTime / 24f);
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            currentTime += (Time.deltaTime / dayDuration) * 24f;
            if (currentTime >= 24)
                currentTime = 0;
        }
        else
        {
            currentTime = startTime;
        }

        float timeProgress = currentTime / 24f;
        UpdateLighting(timeProgress);
    }

    private void UpdateLighting(float timeProgress)
    {
        // Update sun rotation
        float sunRotation = timeProgress * 360f;
        sun.transform.localRotation = Quaternion.Euler(new Vector3(sunRotation - 90, 170, 0));

        // Calculate time of day (0-1 where 0.5 is noon)
        float timeOfDay = Mathf.Abs(timeProgress * 2 - 1); // 0 at noon, 1 at midnight

        // Calculate intensity multiplier with sharper night transition
        float intensityMultiplier;
        if (currentTime <= 12) // Morning
        {
            intensityMultiplier = Mathf.Lerp(nightDarkness, 1, Mathf.Pow(currentTime / 12, nightFadeSpeed));
        }
        else // Evening
        {
            intensityMultiplier = Mathf.Lerp(1, nightDarkness, Mathf.Pow((currentTime - 12) / 12, nightFadeSpeed));
        }

        // Update sun intensity with more dramatic night reduction
        sun.intensity = sunBaseIntensity * intensityMultiplier * sunVariation;

        // Update sun color
        sun.color = sunColor.Evaluate(timeProgress);

        // Update ambient lighting with smoother transition
        Color targetAmbientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, intensityMultiplier);
        RenderSettings.ambientLight = targetAmbientLight;

        if (skyboxMaterial != null)
        {
            // Update skybox parameters
            skyboxMaterial.SetFloat("_Rotation", sunRotation);

            // Adjust skybox parameters for darker nights
            if (skyboxMaterial.HasProperty("_SunSize"))
                skyboxMaterial.SetFloat("_SunSize", Mathf.Lerp(0.04f, 0.08f, intensityMultiplier));

            if (skyboxMaterial.HasProperty("_AtmosphereThickness"))
                skyboxMaterial.SetFloat("_AtmosphereThickness", Mathf.Lerp(0.5f, 1.2f, intensityMultiplier));

            if (skyboxMaterial.HasProperty("_Exposure"))
            {
                float exposure = Mathf.Lerp(0.1f, 1.3f, intensityMultiplier); // Darker night exposure
                skyboxMaterial.SetFloat("_Exposure", exposure);
            }
        }
    }

    public string GetCurrentTimeString()
    {
        int hours = (int)currentTime;
        int minutes = (int)((currentTime % 1) * 60);
        return string.Format("{0:00}:{1:00}", hours, minutes);
    }
}
