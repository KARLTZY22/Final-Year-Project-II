using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ClueManager : MonoBehaviour
{
    [System.Serializable]
    public class ClueData
    {
        public string title;
        public string content;
        public Sprite icon;
    }

    public static ClueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject cluePanel;           // Panel that contains all clue UI
    public GameObject clueEntryPrefab;     // Prefab for each clue entry
    public Transform clueContainer;        // Where clue entries are spawned
    public GameObject clueDetailPanel;     // Panel for detailed view
    public TextMeshProUGUI detailTitleText;
    public TextMeshProUGUI detailContentText;
    public Image detailIcon;

    [Header("Settings")]
    public KeyCode openClueMenuKey = KeyCode.J;
    public AudioClip openSound;
    public AudioClip closeSound;

    private List<ClueData> collectedClues = new List<ClueData>();
    private AudioSource audioSource;
    private QuestManager questManager;

    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make sure it doesn't get destroyed on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();

        // Make sure UI starts hidden
        if (cluePanel != null)
        {
            cluePanel.SetActive(false);
        }
        if (clueDetailPanel != null)
        {
            clueDetailPanel.SetActive(false);
        }

        // Validate required components
        if (cluePanel == null) Debug.LogError("Clue Panel not assigned in ClueManager!");
        if (clueEntryPrefab == null) Debug.LogError("Clue Entry Prefab not assigned in ClueManager!");
        if (clueContainer == null) Debug.LogError("Clue Container not assigned in ClueManager!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(openClueMenuKey))
        {
            ToggleCluePanel();
        }
    }

    public void AddClue(string title, string content, Sprite icon)
    {
        Debug.Log($"Adding clue: {title}"); // Debug log

        ClueData newClue = new ClueData
        {
            title = title,
            content = content,
            icon = icon
        };

        collectedClues.Add(newClue);

        if (questManager != null)
        {
            questManager.UpdateClueCount(collectedClues.Count);
        }

        UpdateClueUI();
    }

    private void UpdateClueUI()
    {
        if (clueContainer == null) return;

        // Clear existing entries
        foreach (Transform child in clueContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new entries
        foreach (var clue in collectedClues)
        {
            GameObject entry = Instantiate(clueEntryPrefab, clueContainer);
            ClueEntryItem clueEntry = entry.GetComponent<ClueEntryItem>();

            if (clueEntry != null)
            {
                clueEntry.SetupClueEntry(clue.title, clue.content, clue.icon);

                // Add button listener
                Button button = entry.GetComponent<Button>();
                if (button != null)
                {
                    ClueData capturedClue = clue; // Capture the current clue for the lambda
                    button.onClick.AddListener(() => ShowClueDetail(capturedClue));
                }
            }
        }
    }

    private void ShowClueDetail(ClueData clue)
    {
        if (clueDetailPanel != null)
        {
            clueDetailPanel.SetActive(true);

            if (detailTitleText != null)
                detailTitleText.text = clue.title;

            if (detailContentText != null)
                detailContentText.text = clue.content;

            if (detailIcon != null && clue.icon != null)
                detailIcon.sprite = clue.icon;
        }
    }

    public void CloseClueDetail()
    {
        if (clueDetailPanel != null)
            clueDetailPanel.SetActive(false);
    }

    private void ToggleCluePanel()
    {
        Debug.Log("Toggling clue panel"); // Debug log

        if (cluePanel != null)
        {
            bool isActive = !cluePanel.activeSelf;
            cluePanel.SetActive(isActive);

            if (isActive)
            {
                UpdateClueUI();
                if (openSound != null && audioSource != null)
                    audioSource.PlayOneShot(openSound);
            }
            else
            {
                CloseClueDetail();
                if (closeSound != null && audioSource != null)
                    audioSource.PlayOneShot(closeSound);
            }
        }
        else
        {
            Debug.LogError("Clue Panel reference is missing!");
        }
    }
}
