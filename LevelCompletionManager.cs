using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelCompletionManager : MonoBehaviour
{
    [Header("Level Complete Settings")]
    public string completionTitle = "Level 1 Complete!";
    [TextArea(3, 10)]
    public string completionMessage = "Congratulations! You've completed all objectives. Ready to proceed to Level 2?";

    [Header("Audio")]
    public AudioClip completionSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("Scene Settings")]
    public string nextSceneName = "FinalScene2";
    public float transitionDelay = 1f;

    private QuestManager questManager;
    private AudioSource audioSource;
    private bool isCompleting = false;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get QuestManager
        questManager = GetComponent<QuestManager>();
        if (questManager == null)
        {
            Debug.LogError("QuestManager not found on the same GameObject!");
            enabled = false;
            return;
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && completionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (!isCompleting && questManager.AreAllObjectivesComplete())
        {
            StartCoroutine(HandleLevelCompletion());
        }
    }

    private IEnumerator HandleLevelCompletion()
    {
        isCompleting = true;

        // Play completion sound
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound, soundVolume);
        }

        // Show completion message using TutorialManager
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.QueueMessage(
                completionTitle,
                completionMessage + "\n\nPress SPACE to continue...",
                null,
                true  // Pause the game
            );

            // Wait for spacebar press
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            // Small delay before transition
            yield return new WaitForSecondsRealtime(transitionDelay);

            // Save game state if needed
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel1();
            }
            else
            {
                // Direct scene transition if no GameManager
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            Debug.LogError("TutorialManager not found in the scene!");
            yield return new WaitForSeconds(transitionDelay);
            SceneManager.LoadScene(nextSceneName);
        }
    }
}