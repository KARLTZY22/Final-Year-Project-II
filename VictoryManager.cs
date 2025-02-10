using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryTitle;
    public TextMeshProUGUI victoryMessage;
    public Button mainMenuButton;

    [Header("Audio")]
    public AudioClip victorySound;
    [Range(0f, 1f)]
    public float audioVolume = 1f;

    private AudioSource audioSource;

    private void Start()
    {
        // Hide victory panel at start
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Setup main menu button
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    public void ShowVictory()
    {
        // Show panel
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // Play victory sound
        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound, audioVolume);
        }

        // Stop time
        Time.timeScale = 0f;

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Main Menu scene
    }

    private void OnDestroy()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
    }
}
