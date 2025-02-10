using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI subText;
    public float fadeInDuration = 1f;

    [Header("Audio")]
    public AudioClip gameOverSound;
    [Range(0f, 1f)]
    public float audioVolume;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public float delayBeforeInput = 1f;

    private AudioSource audioSource;
    private bool isGameOver = false;
    private bool canReturnToMenu = false;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        InitializeComponents();
        SubscribeToPlayerEvents();
    }

    private void InitializeComponents()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Get canvas group or add one
        canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();

        // Hide panel at start
        gameOverPanel.SetActive(false);
        canvasGroup.alpha = 0;
    }

    private void SubscribeToPlayerEvents()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += HandleGameOver;
        }
    }

    private void Update()
    {
        if (isGameOver && canReturnToMenu)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReturnToMainMenu();
            }
        }
    }

    public void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Pause game
        Time.timeScale = 0f;

        // Show UI
        StartCoroutine(ShowGameOverSequence());
    }

    private IEnumerator ShowGameOverSequence()
    {
        // Enable panel
        gameOverPanel.SetActive(true);

        // Fade in
        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // Play sound
        if (gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound, audioVolume);
        }

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Wait before allowing input
        yield return new WaitForSecondsRealtime(delayBeforeInput);
        canReturnToMenu = true;

        // Update text to show can press space
        if (subText != null)
        {
            subText.text = "Press SPACE to return to Main Menu";
        }
    }

    public void ReturnToMainMenu()
    {
        // Reset time scale
        Time.timeScale = 1f;

        // Load main menu
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= HandleGameOver;
        }
    }
}