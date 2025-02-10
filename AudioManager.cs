using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    public AudioMixer masterMixer;
    public float atmosphericVolume = 0.5f;  // Start at 50%

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

    private void Start()
    {
        // Set initial volume
        SetAtmosphericVolume(atmosphericVolume);
    }

    private void LoadSettings()
    {
        atmosphericVolume = PlayerPrefs.GetFloat("AtmosphericVolume", 0.5f);
        UpdateAtmosphericSources();
    }

    public void SetAtmosphericVolume(float volume)
    {
        atmosphericVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("AtmosphericVolume", atmosphericVolume);
        PlayerPrefs.Save();
        UpdateAtmosphericSources();
    }

    public void UpdateAtmosphericSources()
    {
        AtmosphericAudio[] sources = FindObjectsByType<AtmosphericAudio>(FindObjectsSortMode.None);
        foreach (var source in sources)
        {
            if (source != null)
            {
                source.SetVolume(atmosphericVolume);
            }
        }
    }
}
