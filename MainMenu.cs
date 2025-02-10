using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // For UI elements like Slider
using TMPro;  // For TextMeshPro UI elements
using System;  // For Serializable

public class MainMenu : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("Volume Controls")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    [Header("Audio")]
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    private void Start()
    {
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Initialize menu
        ReturnToMainMenu();

        // Set up volume
        InitializeVolume();

        // Reset any existing game state
        ResetGame();
    }


    public void ReturnToMainMenu()
    {
        PlayButtonSound();
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    private void InitializeVolume()
    {
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume", 0.5f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            UpdateVolumeText(savedVolume);
        }
    }

    private void ResetGame()
    {
        // Clear saved data
        PlayerPrefs.DeleteAll();

        // Reset any managers
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null)
        {
            Destroy(tutorialManager.gameObject);
        }

        // Reset game state
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGame()
    {
        PlayButtonSound();
        ResetGame();
        SceneManager.LoadScene(1); // Load FinalScene1
    }

    public void OpenOptions()
    {
        PlayButtonSound();
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        PlayButtonSound();
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        PlayButtonSound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            volumeText.text = $"Volume: {(value * 100):F0}%";
        }
    }

    private void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}