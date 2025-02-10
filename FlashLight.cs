using UnityEngine;

public class FlashLight : MonoBehaviour
{
    [Header("Light Settings")]
    public Light spotLight;
    public float lightIntensity = 2.5f;
    public KeyCode toggleKey = KeyCode.F;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip toggleSound;

    private bool isOn = false;

    void Start()
    {
        // Make sure light is off at start
        if (spotLight != null)
        {
            spotLight.enabled = false;
        }
    }

    void Update()
    {
        // Toggle flashlight when pressing F
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }

    void ToggleFlashlight()
    {
        isOn = !isOn;

        if (spotLight != null)
        {
            spotLight.enabled = isOn;
            spotLight.intensity = lightIntensity;
        }

        // Play toggle sound if available
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }
    }
}
