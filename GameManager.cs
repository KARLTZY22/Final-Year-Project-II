using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public class PlayerState
{
    public float health;
    public int trashCount;
    public int enemiesDefeated;
    public int cluesFound;
    public bool inScene2;
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
        private set { instance = value; }
    }

    [Header("Scene Settings")]
    [SerializeField] private bool isInScene2 = false;  // Keep this private
    [SerializeField] private string scene2Name = "FinalScene2";

    [Header("Player Settings")]
    [SerializeField] private float startingHealth = 100f;
    [SerializeField] private Vector3 scene2PlayerSpawn = new Vector3(0, 1, 0);

    [Header("UI References")]
    [SerializeField] private GameObject scene2ObjectivesUI;
    [SerializeField] private GameObject trashCollectionUI;
    [SerializeField] private GameObject weaponSystemUI;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip sceneChangeSound;
    [SerializeField][Range(0f, 1f)] private float audioVolume = 0.7f;

    private PlayerState gameState = new PlayerState();
    private AudioSource audioPlayer;
    private bool isChangingScene = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitNewGame();
            SetupComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitNewGame()
    {
        Debug.Log("Starting new game");
        isInScene2 = false;
        PlayerPrefs.DeleteAll();
        gameState = new PlayerState
        {
            health = startingHealth,
            inScene2 = false,
            trashCount = 0,
            enemiesDefeated = 0,
            cluesFound = 0
        };
        SaveGame();
    }

    private void SetupComponents()
    {
        audioPlayer = gameObject.AddComponent<AudioSource>();
        audioPlayer.playOnAwake = false;
        SceneManager.sceneLoaded += OnNewSceneLoaded;
    }

    public void FinishScene1()
    {
        if (isChangingScene) return;
        isChangingScene = true;

        SaveCurrentState();

        if (sceneChangeSound != null && audioPlayer != null)
        {
            audioPlayer.PlayOneShot(sceneChangeSound, audioVolume);
        }

        isInScene2 = true;
        gameState.inScene2 = true;

        TransitionToScene2();
    }

    private void TransitionToScene2()
    {
        var transitionSystem = FindFirstObjectByType<SceneTransitionManager>();
        if (transitionSystem != null)
        {
            transitionSystem.TransitionToScene(scene2Name);
        }
        else
        {
            SceneManager.LoadScene(scene2Name);
        }
    }

    private void SaveCurrentState()
    {
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        var questSystem = FindFirstObjectByType<QuestManager>();

        if (playerHealth != null)
        {
            gameState.health = playerHealth.currentHealth;
        }

        if (questSystem != null)
        {
            gameState.trashCount = questSystem.TotalTrashCollected;
            gameState.enemiesDefeated = questSystem.CurrentEnemies;
            gameState.cluesFound = questSystem.CurrentClues;
        }

        SaveGame();
    }

    private void OnNewSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        Debug.Log($"Loading scene: {sceneName}");

        if (sceneName == "FinalScene1")
        {
            isInScene2 = false;
            gameState.inScene2 = false;
            InitNewGame();
        }
        else if (sceneName == scene2Name)
        {
            SetupScene2();
        }

        isChangingScene = false;
        UpdateUIElements();
    }

    private void SetupScene2()
    {
        Debug.Log("Setting up Scene 2");
        if (trashCollectionUI != null) trashCollectionUI.SetActive(false);
        if (scene2ObjectivesUI != null) scene2ObjectivesUI.SetActive(true);
        if (weaponSystemUI != null) weaponSystemUI.SetActive(true);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = scene2PlayerSpawn;

            var healthComponent = player.GetComponent<PlayerHealth>();
            if (healthComponent != null)
            {
                healthComponent.currentHealth = gameState.health;
            }
        }
    }

    private void UpdateUIElements()
    {
        if (isInScene2)
        {
            if (trashCollectionUI != null) trashCollectionUI.SetActive(false);
            if (scene2ObjectivesUI != null) scene2ObjectivesUI.SetActive(true);
        }
        else
        {
            if (trashCollectionUI != null) trashCollectionUI.SetActive(true);
            if (scene2ObjectivesUI != null) scene2ObjectivesUI.SetActive(false);
        }
    }

    private void SaveGame()
    {
        string savedData = JsonUtility.ToJson(gameState);
        PlayerPrefs.SetString("SavedGame", savedData);
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedGame"))
        {
            string savedData = PlayerPrefs.GetString("SavedGame");
            gameState = JsonUtility.FromJson<PlayerState>(savedData);
            isInScene2 = gameState.inScene2;
        }
    }

    public float GetPlayerHealth()
    {
        return gameState.health;
    }

    // Public method to access the state of isInScene2
    public bool IsScene2() { return isInScene2; }  // Method to access the value

    public void SaveGameState()
    {
        string savedData = JsonUtility.ToJson(gameState);
        PlayerPrefs.SetString("SavedGame", savedData);
        PlayerPrefs.Save();
    }

    public void CompleteLevel1()
    {
        if (isChangingScene) return;
        isChangingScene = true;
        SaveCurrentState();
        isInScene2 = true;
        gameState.inScene2 = true;
        SceneManager.LoadScene(scene2Name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnNewSceneLoaded;
    }
}
