using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;

    [Header("Scene Settings")]
    private const int MAIN_MENU_INDEX = 0;
    private const int SCENE1_INDEX = 1;
    private const int SCENE2_INDEX = 2;

    [Header("Player References")]
    private FirstPersonController firstPersonController;
    private WeaponManagerScene2 weaponManager2;
    private WeaponManager weaponManager1;

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    private bool isPaused = false;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Hide pause menu at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Find and cache player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            firstPersonController = player.GetComponent<FirstPersonController>();
            weaponManager2 = player.GetComponentInChildren<WeaponManagerScene2>();
            weaponManager1 = player.GetComponentInChildren<WeaponManager>();

            // Log component findings for debugging
            Debug.Log("Found Player Components: " +
                     "FPS Controller: " + (firstPersonController != null) +
                     ", Weapon Manager 2: " + (weaponManager2 != null) +
                     ", Weapon Manager 1: " + (weaponManager1 != null));
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure player has the 'Player' tag.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        // Disable player movement and input
        if (firstPersonController != null)
        {
            firstPersonController.enabled = false;

            // Disable input system
            var playerInput = firstPersonController.GetComponent<StarterAssetsInputs>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
        }

        // Disable weapons based on current scene
        if (weaponManager2 != null)
        {
            weaponManager2.enabled = false;
        }
        if (weaponManager1 != null)
        {
            weaponManager1.enabled = false;
        }

        // Show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Show and unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        // Re-enable player movement and input
        if (firstPersonController != null)
        {
            firstPersonController.enabled = true;

            // Re-enable input system
            var playerInput = firstPersonController.GetComponent<StarterAssetsInputs>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
        }

        // Re-enable weapons based on current scene
        if (weaponManager2 != null)
        {
            weaponManager2.enabled = true;
        }
        if (weaponManager1 != null)
        {
            weaponManager1.enabled = true;
        }

        // Hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        isPaused = false;

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Game Resumed");
    }

    public void ReturnToMainMenu()
    {
        // Resume normal time scale
        Time.timeScale = 1f;

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear game state
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        // Clear any persisting data
        PlayerPrefs.DeleteKey("GameState");
        PlayerPrefs.Save();

        // Load main menu scene
        SceneManager.LoadScene(0); // M
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Ensure normal state is restored when object is destroyed
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}