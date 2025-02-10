using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Scene2Manager : MonoBehaviour
{
    [Header("Objectives")]
    public GameObject boss;
    public GameObject machine;

    [Header("UI Elements")]
    public TextMeshProUGUI objectivesText;
    public GameObject victoryPanel;
    public TextMeshProUGUI notificationText;

    [Header("Sound Effects")]
    public AudioClip objectiveCompletedSound;
    public AudioClip victorySound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("Effects")]
    public GameObject victoryEffect;
    public float notificationDuration = 3f;

    // References
    private AudioSource audioSource;

    // State tracking
    private bool bossDefeated;
    private bool machineDestroyed;
    private bool hasShownVictory;

    private void Start()
    {
        InitializeComponents();
        InitializeScene2();
    }

    private void InitializeComponents()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Initialize UI
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        UpdateObjectivesUI();
        ShowNotification("Defeat the Boss and Destroy the Machine!");
    }

    public void InitializeScene2()
    {
        bossDefeated = false;
        machineDestroyed = false;
        hasShownVictory = false;
        UpdateObjectivesUI();
    }

    public void OnBossDefeated()
    {
        if (!bossDefeated)
        {
            bossDefeated = true;
            PlaySound(objectiveCompletedSound);
            ShowNotification("Boss Defeated!");
            UpdateObjectivesUI();
            CheckVictoryCondition();
        }
    }

    public void OnMachineDestroyed()
    {
        if (!machineDestroyed)
        {
            machineDestroyed = true;
            PlaySound(objectiveCompletedSound);
            ShowNotification("Machine Destroyed!");
            UpdateObjectivesUI();
            CheckVictoryCondition();
        }
    }

    private void UpdateObjectivesUI()
    {
        if (objectivesText != null)
        {
            string bossStatus = bossDefeated ? "✓" : "□";
            string machineStatus = machineDestroyed ? "✓" : "□";

            objectivesText.text = $"Scene 2 Objectives:\n" +
                                $"Defeat Boss [{bossStatus}]\n" +
                                $"Destroy Machine [{machineStatus}]";
        }
    }

    private void CheckVictoryCondition()
    {
        if (hasShownVictory) return;

        bool allObjectivesComplete = bossDefeated && machineDestroyed;

        if (allObjectivesComplete)
        {
            hasShownVictory = true;
            StartCoroutine(HandleVictory());
        }
    }

    private IEnumerator HandleVictory()
    {
        PlaySound(victorySound);

        ShowNotification("All Objectives Complete!");

        if (victoryEffect != null)
        {
            Instantiate(victoryEffect, Camera.main.transform.position + Camera.main.transform.forward * 2f, Quaternion.identity);
        }

        yield return new WaitForSeconds(2f);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            CancelInvoke(nameof(ClearNotification));
            Invoke(nameof(ClearNotification), notificationDuration);
        }
    }

    private void ClearNotification()
    {
        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }

    // Public methods for other scripts
    public bool IsBossDefeated() => bossDefeated;
    public bool IsMachineDestroyed() => machineDestroyed;
    public bool IsComplete() => hasShownVictory;
}