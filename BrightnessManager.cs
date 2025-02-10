using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BrightnessManager : MonoBehaviour
{
    public static BrightnessManager Instance { get; private set; }

    [Range(0f, 2f)]
    public float brightness = 1f; // Default brightness

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSettings()
    {
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        UpdateBrightness();
    }

    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp(value, 0f, 2f);
        PlayerPrefs.SetFloat("Brightness", brightness);
        PlayerPrefs.Save();
        UpdateBrightness();
    }

    public void UpdateBrightness()
    {
        // Find and update all brightness controllers in the scene
        BrightnessController[] controllers = FindObjectsByType<BrightnessController>(FindObjectsSortMode.None);
        foreach (var controller in controllers)
        {
            if (controller != null)
            {
                controller.ApplyBrightness(brightness);
            }
        }
    }
}
