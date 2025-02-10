using UnityEngine;
using TMPro;

public class KeyItem : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float promptDistance = 3f;

    private QuestManager questManager;
    private bool isCollected = false;
    private bool playerInRange = false;
    private AudioSource audioSource;
    private TextMeshProUGUI promptText;
    private GameObject promptObject;

    void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();

        // Setup audio
        SetupAudio();

        // Create prompt UI
        CreatePromptUI();
    }

    void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.volume = pickupVolume;
            audioSource.clip = pickupSound;
        }
    }

    void CreatePromptUI()
    {
        // Create a new GameObject for the prompt
        promptObject = new GameObject("KeyPrompt");
        promptObject.transform.SetParent(GameObject.Find("Canvas").transform);

        // Add TextMeshProUGUI component
        promptText = promptObject.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press E to collect key";
        promptText.fontSize = 24;
        promptText.alignment = TextAlignmentOptions.Center;

        // Position the text
        RectTransform rectTransform = promptText.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 100); // Position from center
        rectTransform.sizeDelta = new Vector2(300, 50); // Width and height
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Hide initially
        promptText.enabled = false;
    }

    void Update()
    {
        if (playerInRange && !isCollected && Input.GetKeyDown(interactKey))
        {
            CollectKey();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptText != null)
            {
                promptText.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptText != null)
            {
                promptText.enabled = false;
            }
        }
    }

    private void CollectKey()
    {
        isCollected = true;

        // Play pickup sound
        if (audioSource != null && pickupSound != null)
        {
            // Play the sound at the key's position
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }

        // Update quest
        if (questManager != null)
        {
            questManager.OnKeyFound();
        }

        // Hide prompt
        if (promptText != null)
        {
            Destroy(promptObject);
        }

        // Disable the key object
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up UI when destroyed
        if (promptObject != null)
        {
            Destroy(promptObject);
        }
    }
}
