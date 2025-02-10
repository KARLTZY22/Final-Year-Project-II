using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AtmosphericAudio : MonoBehaviour
{
    [Header("Audio Mode")]
    public bool isMenuMusic = false;

    [Header("Single Track Settings")]
    public AudioClip menuMusic;

    [Header("Multi Track Settings")]
    public AudioClip[] atmosphericSounds;
    public bool randomizePlayback = true;
    public bool playMultipleSounds = false;

    [Header("Common Settings")]
    [Range(0f, 1f)]
    public float atmosphereVolume = 0.5f;
    public AudioMixerGroup mixerGroup;

    [Header("Fade Settings")]
    public float fadeInDuration = 2f;
    public float crossFadeDuration = 3f;

    private AudioSource[] audioSources;
    private int currentTrack = 0;
    private float initialVolume;
    private bool isCrossFading = false;

    private void Awake()
    {
        initialVolume = atmosphereVolume;

        // Get volume from AudioManager if it exists
        if (AudioManager.Instance != null)
        {
            atmosphereVolume *= AudioManager.Instance.atmosphericVolume;
        }

        SetupAudioSources();
    }

    private void Start()
    {
        StartAtmosphere();
    }

    private void OnEnable()
    {
        // Update volume when enabled
        if (AudioManager.Instance != null)
        {
            SetVolume(AudioManager.Instance.atmosphericVolume);
        }
    }

    private void SetupAudioSources()
    {
        if (isMenuMusic)
        {
            // Setup single source for menu music
            audioSources = new AudioSource[1];
            AudioSource source = gameObject.AddComponent<AudioSource>();
            SetupAudioSource(source);
            source.clip = menuMusic;
            audioSources[0] = source;
        }
        else
        {
            if (atmosphericSounds == null || atmosphericSounds.Length == 0)
            {
                Debug.LogWarning("No atmospheric sounds assigned!");
                return;
            }

            if (playMultipleSounds)
            {
                audioSources = new AudioSource[atmosphericSounds.Length];
                for (int i = 0; i < atmosphericSounds.Length; i++)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    SetupAudioSource(source);
                    source.clip = atmosphericSounds[i];
                    audioSources[i] = source;
                }
            }
            else
            {
                audioSources = new AudioSource[1];
                AudioSource source = gameObject.AddComponent<AudioSource>();
                SetupAudioSource(source);
                audioSources[0] = source;
                source.clip = atmosphericSounds[currentTrack];
            }
        }
    }

    private void SetupAudioSource(AudioSource source)
    {
        source.loop = true;
        source.volume = atmosphereVolume;
        source.spatialBlend = 0f;
        source.priority = 256;
        source.playOnAwake = false;

        if (mixerGroup != null)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }
    }

    private void StartAtmosphere()
    {
        if (audioSources == null || audioSources.Length == 0) return;

        if (isMenuMusic)
        {
            if (audioSources[0] != null && menuMusic != null)
            {
                audioSources[0].Play();
                StartCoroutine(FadeIn(audioSources[0]));
            }
        }
        else
        {
            if (playMultipleSounds)
            {
                foreach (var source in audioSources)
                {
                    if (source != null)
                    {
                        source.Play();
                        StartCoroutine(FadeIn(source));
                    }
                }
            }
            else
            {
                audioSources[0].Play();
                StartCoroutine(FadeIn(audioSources[0]));
            }
        }
    }

    public void SetVolume(float volume)
    {
        atmosphereVolume = Mathf.Clamp01(volume * initialVolume);
        if (audioSources != null)
        {
            foreach (var source in audioSources)
            {
                if (source != null && !isCrossFading)
                {
                    source.volume = atmosphereVolume;
                }
            }
        }
    }

    private IEnumerator FadeIn(AudioSource source)
    {
        float startVolume = 0f;
        float targetVolume = atmosphereVolume;
        float elapsedTime = 0f;

        source.volume = startVolume;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeInDuration);
            source.volume = newVolume;
            yield return null;
        }

        source.volume = targetVolume;
    }

    private void OnDisable()
    {
        if (audioSources != null)
        {
            foreach (var source in audioSources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (audioSources != null)
        {
            foreach (var source in audioSources)
            {
                if (source != null)
                {
                    Destroy(source);
                }
            }
        }
    }
}

