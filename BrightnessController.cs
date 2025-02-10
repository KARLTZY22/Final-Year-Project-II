using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(Light))]
public class BrightnessController : MonoBehaviour
{
    [Header("Brightness Settings")]
    public Light mainLight; // Reference to the main directional light
    public PostProcessVolume postProcessVolume; // Optional post-processing reference

    private float defaultLightIntensity;
    private float defaultExposure;

    private void Awake()
    {
        // Get the main light reference if not assigned
        if (mainLight == null)
        {
            mainLight = GetComponent<Light>();
        }

        // Store default values
        if (mainLight != null)
        {
            defaultLightIntensity = mainLight.intensity;
        }

        // Store post-processing default values if available
        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out ColorGrading colorGrading))
        {
            defaultExposure = colorGrading.postExposure.value;
        }
    }

    private void Start()
    {
        // Apply saved brightness on start
        if (BrightnessManager.Instance != null)
        {
            ApplyBrightness(BrightnessManager.Instance.brightness);
        }
    }

    private void OnEnable()
    {
        // Update brightness when enabled
        if (BrightnessManager.Instance != null)
        {
            ApplyBrightness(BrightnessManager.Instance.brightness);
        }
    }

    public void ApplyBrightness(float brightnessValue)
    {
        // Adjust main light intensity
        if (mainLight != null)
        {
            mainLight.intensity = defaultLightIntensity * brightnessValue;
        }

        // Adjust post-processing exposure if available
        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out ColorGrading colorGrading))
        {
            colorGrading.postExposure.value = defaultExposure + (brightnessValue - 1f);
        }
    }
}
